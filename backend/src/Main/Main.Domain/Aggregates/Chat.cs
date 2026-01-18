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

    public string Title { get; private set; } = string.Empty;

    public string ModelId { get; private set; } = string.Empty;

    public bool IsArchived { get; private set; }

    public int NextSequenceNumber { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Message> Messages => [.. _messages];

    private Chat() { } // For EF Core

    [SetsRequiredMembers]
    private Chat
    (
        ChatId id,
        Guid userId,
        string title,
        string modelId,
        DateTimeOffset utcNow
    )
    {
        Id = id;
        UserId = userId;
        Title = title;
        ModelId = modelId;
        IsArchived = false;
        NextSequenceNumber = 0;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public static Outcome<Chat> Create
    (
        ChatId id,
        Guid userId,
        string title,
        string modelId,
        DateTimeOffset utcNow
    )
    {
        if (userId == Guid.Empty)
            return ChatFaults.UserIdRequired;

        if (string.IsNullOrWhiteSpace(title))
            return ChatFaults.TitleRequired;

        if (title.Length > ChatConstants.MaxTitleLength)
            return ChatFaults.TitleTooLong;
        
        if (string.IsNullOrWhiteSpace(modelId))
            return ChatFaults.ModelIdRequired;

        Chat chat = new
        (
            id: id,
            userId: userId,
            title: title,
            modelId: modelId,
            utcNow: utcNow
        );

        return chat;
    }

    public Outcome RenameTitle(string newTitle, DateTimeOffset utcNow)
    {
        if (IsArchived)
            return ChatFaults.CannotModifyArchivedChat;

        if (string.IsNullOrWhiteSpace(newTitle))
            return ChatFaults.TitleRequired;

        if (newTitle.Length > ChatConstants.MaxTitleLength)
            return ChatFaults.TitleTooLong;

        Title = newTitle;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }

    public Outcome Archive(DateTimeOffset utcNow)
    {
        if (IsArchived)
            return ChatFaults.AlreadyArchived;

        IsArchived = true;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }

    public Outcome Unarchive(DateTimeOffset utcNow)
    {
        if (!IsArchived)
            return ChatFaults.NotArchived;

        IsArchived = false;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }

    private Outcome<Message> AddMessage
    (
        MessageId messageId,
        string messageContent,
        MessageRole role,
        DateTimeOffset utcNow
    )
    {
        if (IsArchived)
            return ChatFaults.CannotModifyArchivedChat;

        int sequenceNumber = NextSequenceNumber;
        NextSequenceNumber++;

        Outcome<Message> messageOutcome = Message.Create
        (
            id: messageId,
            chatId: Id,
            messageRole: role,
            messageContent: messageContent,
            sequenceNumber: sequenceNumber,
            utcNow: utcNow
        );

        if (messageOutcome.IsFailure)
            return messageOutcome.Fault;

        Message message = messageOutcome.Value;

        _messages.Add(message);
        UpdatedAt = utcNow;

        return message;
    }

    public Outcome<Message> AddUserMessage(MessageId messageId, string messageContent, DateTimeOffset utcNow)
        => AddMessage(messageId, messageContent, MessageRole.User, utcNow);

    public Outcome<Message> AddAssistantMessage(MessageId messageId, string messageContent, DateTimeOffset utcNow) =>
        AddMessage(messageId, messageContent, MessageRole.Assistant, utcNow);
}