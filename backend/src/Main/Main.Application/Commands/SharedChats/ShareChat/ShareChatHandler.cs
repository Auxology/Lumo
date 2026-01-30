using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.SharedChats.ShareChat;

internal sealed class ShareChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IIdGenerator idGenerator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ShareChatCommand, ShareChatResponse>
{
    public async ValueTask<Outcome<ShareChatResponse>> Handle(ShareChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Outcome<ChatId> chatIdOutcome = ChatId.From(request.ChatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId chatId = chatIdOutcome.Value;

        Chat? chat = await dbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId, cancellationToken: cancellationToken);

        if (chat is null)
            return ChatOperationFaults.NotFound;

        SharedChatId sharedChatId = idGenerator.NewSharedChatId();

        Outcome<SharedChat> sharedChatOutcome = SharedChat.Create
        (
            id: sharedChatId,
            sourceChatId: chat.Id,
            ownerId: userId,
            title: chat.Title,
            modelId: chat.ModelId,
            utcNow: dateTimeProvider.UtcNow
        );

        if (sharedChatOutcome.IsFailure)
            return sharedChatOutcome.Fault;

        SharedChat sharedChat = sharedChatOutcome.Value;

        IReadOnlyList<SharedChatMessage> sharedMessages = chat.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new SharedChatMessage
            (
                SequenceNumber: m.SequenceNumber,
                MessageRole: m.MessageRole,
                MessageContent: m.MessageContent,
                CreatedAt: m.CreatedAt
            ))
            .ToList();

        sharedChat.AddMessages(sharedMessages, dateTimeProvider.UtcNow);

        await dbContext.SharedChats.AddAsync(sharedChat, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        ShareChatResponse response = new
        (
            SharedChatId: sharedChat.Id.Value,
            SourceChatId: sharedChat.SourceChatId.Value,
            OwnerId: sharedChat.OwnerId,
            Title: sharedChat.Title,
            ModelId: sharedChat.ModelId,
            SnapshotAt: sharedChat.SnapshotAt,
            CreatedAt: sharedChat.CreatedAt
        );

        return response;
    }
}