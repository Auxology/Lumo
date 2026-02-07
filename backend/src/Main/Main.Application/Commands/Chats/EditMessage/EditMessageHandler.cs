using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Application.Abstractions.Stream;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.Entities;
using Main.Domain.ReadModels;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.EditMessage;

internal sealed class EditMessageHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    IChatLockService chatLockService,
    IIdGenerator idGenerator,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<EditMessageCommand, EditMessageResponse>
{
    public async ValueTask<Outcome<EditMessageResponse>> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        Outcome<ChatId> chatIdOutcome = ChatId.From(request.ChatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId chatId = chatIdOutcome.Value;

        Outcome<MessageId> messageIdOutcome = MessageId.From(request.MessageId);

        if (messageIdOutcome.IsFailure)
            return messageIdOutcome.Fault;

        MessageId messageId = messageIdOutcome.Value;

        // If very big chats are expected, change approach
        Chat? chat = await dbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId, cancellationToken);

        if (chat is null)
            return ChatOperationFaults.NotFound;

        Message? targetMessage = chat.Messages.FirstOrDefault(m => m.Id == messageId);

        if (targetMessage is null)
            return MessageOperationFaults.NotFound;

        bool lockAcquired = await chatLockService.TryAcquireLockAsync
        (
            chatId: chat.Id.Value,
            ownerId: requestContext.CorrelationId,
            cancellationToken: cancellationToken
        );

        if (!lockAcquired)
            return ChatOperationFaults.GenerationInProgress;

        Outcome editOutcome = chat.EditMessageAndRemoveSubsequent
        (
            messageId: messageId,
            newContent: request.NewContent,
            utcNow: dateTimeProvider.UtcNow
        );

        if (editOutcome.IsFailure)
        {
            await chatLockService.ReleaseLockAsync
            (
                chatId: chat.Id.Value,
                ownerId: requestContext.CorrelationId,
                cancellationToken: cancellationToken
            );
            return editOutcome.Fault;
        }

        try
        {
            StreamId streamId = idGenerator.NewStreamId();

            MessageSent messageSent = new()
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.Parse(requestContext.CorrelationId),
                ChatId = chat.Id.Value,
                UserId = user.UserId,
                StreamId = streamId.Value,
                ModelId = chat.ModelId,
                Message = request.NewContent
            };

            await messageBus.PublishAsync(messageSent, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            EditMessageResponse response = new
            (
                MessageId: targetMessage.Id.Value,
                ChatId: chat.Id.Value,
                StreamId: streamId.Value,
                MessageRole: targetMessage.MessageRole.ToString(),
                MessageContent: targetMessage.MessageContent,
                EditedAt: targetMessage.EditedAt
            );

            return response;
        }
        catch
        {
            await chatLockService.ReleaseLockAsync
            (
                chatId: chat.Id.Value,
                ownerId: requestContext.CorrelationId,
                cancellationToken: cancellationToken
            );
            throw;
        }
    }
}