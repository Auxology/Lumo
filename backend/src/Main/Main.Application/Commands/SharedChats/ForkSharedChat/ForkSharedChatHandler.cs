using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
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

namespace Main.Application.Commands.SharedChats.ForkSharedChat;

internal sealed class ForkSharedChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IIdGenerator idGenerator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ForkSharedChatCommand, ForkSharedChatResponse>
{
    public async ValueTask<Outcome<ForkSharedChatResponse>> Handle(ForkSharedChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Outcome<SharedChatId> sharedChatIdOutcome = SharedChatId.From(request.SharedChatId);

        if (sharedChatIdOutcome.IsFailure)
            return sharedChatIdOutcome.Fault;

        SharedChatId sharedChatId = sharedChatIdOutcome.Value;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        SharedChat? sharedChat = await dbContext.SharedChats
            .Include(s => s.SharedChatMessages)
            .FirstOrDefaultAsync(sc => sc.Id == sharedChatId, cancellationToken);

        if (sharedChat is null)
            return SharedChatOperationFaults.NotFound;

        ChatId chatId = idGenerator.NewChatId();

        Outcome<Chat> chatOutcome = Chat.Create
        (
            id: chatId,
            userId: user.UserId,
            title: sharedChat.Title,
            modelId: sharedChat.ModelId,
            utcNow: dateTimeProvider.UtcNow
        );

        if (chatOutcome.IsFailure)
            return chatOutcome.Fault;

        Chat chat = chatOutcome.Value;

        foreach (SharedChatMessage sharedChatMessage in sharedChat.SharedChatMessages.OrderBy(scm => scm.SequenceNumber))
        {
            MessageId messageId = idGenerator.NewMessageId();

            if (sharedChatMessage.MessageRole == MessageRole.User)
            {
                Outcome<Message> messageOutcome = chat.AddUserMessage
                (
                    messageId: messageId,
                    messageContent: sharedChatMessage.MessageContent,
                    utcNow: dateTimeProvider.UtcNow
                );

                if (messageOutcome.IsFailure)
                    return messageOutcome.Fault;
            }
            else if (sharedChatMessage.MessageRole == MessageRole.Assistant)
            {
                Outcome<Message> messageOutcome = chat.AddAssistantMessage
                (
                    messageId: messageId,
                    messageContent: sharedChatMessage.MessageContent,
                    utcNow: dateTimeProvider.UtcNow
                );

                if (messageOutcome.IsFailure)
                    return messageOutcome.Fault;
            }
        }

        await dbContext.Chats.AddAsync(chat, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        ForkSharedChatResponse response = new
        (
            ChatId: chat.Id.Value,
            Title: chat.Title,
            ModelId: chat.ModelId,
            CreatedAt: chat.CreatedAt
        );

        return response;
    }
}