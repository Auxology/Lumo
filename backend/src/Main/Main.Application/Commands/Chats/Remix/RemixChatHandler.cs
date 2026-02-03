using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Application.Abstractions.Stream;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.Entities;
using Main.Domain.Enums;
using Main.Domain.ReadModels;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.Remix;

internal sealed class RemixChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    IModelRegistry modelRegistry,
    IIdGenerator idGenerator,
    IChatLockService chatLockService,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemixChatCommand, RemixChatResponse>
{
    public async ValueTask<Outcome<RemixChatResponse>> Handle(RemixChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Outcome<ChatId> chatIdOutcome = ChatId.From(request.ChatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId chatId = chatIdOutcome.Value;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        bool isAllowed = modelRegistry.IsModelAllowed(request.NewModelId);

        if (!isAllowed)
            return ChatOperationFaults.InvalidModel;

        Chat? originalChat = await dbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId, cancellationToken);

        if (originalChat is null)
            return ChatOperationFaults.NotFound;

        ChatId newChatId = idGenerator.NewChatId();

        Outcome<Chat> newChatOutcome = Chat.Create
        (
            id: newChatId,
            userId: userId,
            modelId: request.NewModelId,
            title: originalChat.Title,
            utcNow: dateTimeProvider.UtcNow
        );

        if (newChatOutcome.IsFailure)
            return newChatOutcome.Fault;

        Chat newChat = newChatOutcome.Value;

        foreach (Message message in originalChat.Messages)
        {
            MessageId messageId = idGenerator.NewMessageId();

            if (message.MessageRole == MessageRole.User)
            {
                Outcome<Message> messageOutcome = newChat.AddUserMessage
                (
                    messageId: messageId,
                    messageContent: message.MessageContent,
                    utcNow: dateTimeProvider.UtcNow
                );
                
                if (messageOutcome.IsFailure)
                    return messageOutcome.Fault;
            }
            else if (message.MessageRole == MessageRole.Assistant)
            {
                Outcome<Message> messageOutcome = newChat.AddAssistantMessage
                (
                    messageId: messageId,
                    messageContent: message.MessageContent,
                    utcNow: dateTimeProvider.UtcNow
                );
                
                if (messageOutcome.IsFailure)
                    return messageOutcome.Fault;
            }
        }

        bool lockAcquired = await chatLockService.TryAcquireLockAsync(newChat.Id.Value, cancellationToken);

        if (!lockAcquired)
            return ChatOperationFaults.GenerationInProgress;
        
        string latestUserMessage = originalChat.Messages                                                                                                                                                                                   
            .Where(m => m.MessageRole == MessageRole.User)                                                                                                                                                                           
            .Select(m => m.MessageContent)                                                                                                                                                                                           
            .LastOrDefault() ?? string.Empty;
        
        try
        {
            StreamId streamId = idGenerator.NewStreamId();

            MessageSent messageSent = new()
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.Parse(requestContext.CorrelationId),
                ChatId = newChat.Id.Value,
                UserId = newChat.UserId,
                StreamId = streamId.Value,
                ModelId = newChat.ModelId,
                Message = latestUserMessage
            };

            await dbContext.Chats.AddAsync(newChat, cancellationToken);
            await messageBus.PublishAsync(messageSent, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            RemixChatResponse response = new
            (
                ChatId: newChat.Id.Value,
                StreamId: streamId.Value,
                ChatTitle: newChat.Title,
                ModelId: newChat.ModelId,
                CreatedAt: newChat.CreatedAt
            );

            return response;
        }
        catch
        {
            await chatLockService.ReleaseLockAsync(newChat.Id.Value, cancellationToken);
            throw;
        }
    }
}