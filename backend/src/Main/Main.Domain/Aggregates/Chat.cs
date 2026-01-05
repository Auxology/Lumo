using System.Diagnostics.CodeAnalysis;

using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.Enums;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Aggregates;

public sealed class Chat : AggregateRoot<ChatId>
{
    private readonly List<Message> _messages = [];

    public Guid UserId { get; private set; }

    public string? Title { get; private set; }

    public string? ModelName { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Message> Messages => [.. _messages];

    private Chat() { } // For EF Core

    [SetsRequiredMembers]
    private Chat
    (
        Guid userId,
        DateTimeOffset utcNow
    )
    {
        Id = ChatId.New();
        UserId = userId;
        Title = null;
        ModelName = null;
        IsArchived = false;
        CreatedAt = utcNow;
        UpdatedAt = null;
    }

    public static Outcome<Chat> Create(Guid userId, DateTimeOffset utcNow)
    {
        if (userId == Guid.Empty)
            return ChatFaults.UserIdRequired;

        Chat chat = new
        (
            userId: userId,
            utcNow: utcNow
        );

        return chat;
    }

    public Outcome AddTitle(string title, DateTimeOffset utcNow)
    {
        if (IsArchived)
            return ChatFaults.CannotModifyArchivedChat;

        if (string.IsNullOrWhiteSpace(title))
            return ChatFaults.TitleRequired;

        if (title.Length > ChatConstants.MaxTitleLength)
            return ChatFaults.TitleTooLong;

        Title = title;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }

    public bool Archive(DateTimeOffset utcNow)
    {
        if (IsArchived)
            return false;

        IsArchived = true;
        UpdatedAt = utcNow;

        return true;
    }

    public Outcome AddUserMessage(string messageContent, DateTimeOffset utcNow)
    {
        if (IsArchived)
            return ChatFaults.CannotModifyArchivedChat;

        Outcome<Message> messageOutcome = Message.Create
        (
            chatId: Id,
            messageRole: MessageRole.User,
            messageContent: messageContent,
            utcNow: utcNow
        );

        if (messageOutcome.IsFailure)
            return messageOutcome.Fault;

        Message message = messageOutcome.Value;

        _messages.Add(message);
        UpdatedAt = utcNow;

        return Outcome.Success();
    }
    
    public Outcome AddAssistantMessage(string messageContent, DateTimeOffset utcNow)
    {
        if (IsArchived)
            return ChatFaults.CannotModifyArchivedChat;

        Outcome<Message> messageOutcome = Message.Create
        (
            chatId: Id,
            messageRole: MessageRole.Assistant,
            messageContent: messageContent,
            utcNow: utcNow
        );

        if (messageOutcome.IsFailure)
            return messageOutcome.Fault;

        Message message = messageOutcome.Value;

        _messages.Add(message);
        UpdatedAt = utcNow;

        return Outcome.Success();
    }
}