using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Application.Abstractions.Stream;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.ReadModels;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.Start;

internal sealed class StartChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    IModelRegistry modelRegistry,
    IChatLockService chatLockService,
    IIdGenerator idGenerator,
    IChatCompletionService chatCompletionService,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<StartChatCommand, StartChatResponse>
{
    public async ValueTask<Outcome<StartChatResponse>> Handle(StartChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        string modelId = request.ModelId ?? modelRegistry.GetDefaultModelId();

        bool isAllowed = modelRegistry.IsModelAllowed(modelId);

        if (!isAllowed)
            return ChatOperationFaults.InvalidModel;

        string title = await chatCompletionService.GetTitleAsync(request.Message, cancellationToken);

        ChatId chatId = idGenerator.NewChatId();

        Outcome<Chat> chatOutcome = Chat.Create
        (
            id: chatId,
            userId: user.UserId,
            title: title,
            modelId: modelId,
            utcNow: dateTimeProvider.UtcNow
        );

        if (chatOutcome.IsFailure)
            return chatOutcome.Fault;

        Chat chat = chatOutcome.Value;

        MessageId messageId = idGenerator.NewMessageId();

        Outcome messageOutcome = chat.AddUserMessage
        (
            messageId: messageId,
            messageContent: request.Message,
            utcNow: dateTimeProvider.UtcNow
        );

        if (messageOutcome.IsFailure)
            return messageOutcome.Fault;

        bool lockAcquired = await chatLockService.TryAcquireLockAsync
        (
            chatId: chat.Id.Value,
            ownerId: requestContext.CorrelationId,
            cancellationToken: cancellationToken
        );

        if (!lockAcquired)
            return ChatOperationFaults.GenerationInProgress;

        try
        {
            StreamId streamId = idGenerator.NewStreamId();

            ChatStarted chatStarted = new()
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.Parse(requestContext.CorrelationId),
                ChatId = chat.Id.Value,
                UserId = user.UserId,
                StreamId = streamId.Value,
                ModelId = modelId,
                InitialMessage = request.Message
            };

            await dbContext.Chats.AddAsync(chat, cancellationToken);
            await messageBus.PublishAsync(chatStarted, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            StartChatResponse response = new
            (
                ChatId: chat.Id.Value,
                StreamId: streamId.Value,
                ChatTitle: title,
                ModelId: modelId,
                CreatedAt: chat.CreatedAt
            );

            return response;
        }
        catch
        {
            await chatLockService.ReleaseLockAsync
            (
                chatId: chat.Id.Value,
                ownerId: requestContext.CorrelationId,
                cancellationToken
            );
            throw;
        }
    }
}