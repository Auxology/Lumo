using System.Diagnostics.CodeAnalysis;

using Main.Domain.Enums;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Entities;

public sealed class Message : Entity<MessageId>
{
    public ChatId ChatId { get; private set; }

    public MessageRole MessageRole { get; private set; }

    public string MessageContent { get; private set; } = string.Empty;

    public long? TokenCount { get; private set; }
    
    public int SequenceNumber { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Message() { } // For EF Core

    [SetsRequiredMembers]
    private Message
    (
        MessageId id,
        ChatId chatId,
        MessageRole messageRole,
        string messageContent,
        int sequenceNumber,
        DateTimeOffset utcNow
    )
    {
        Id = id;
        ChatId = chatId;
        MessageRole = messageRole;
        MessageContent = messageContent;
        TokenCount = null;
        SequenceNumber = sequenceNumber;
        CreatedAt = utcNow;
    }

    internal static Outcome<Message> Create
    (
        MessageId id,
        ChatId chatId,
        MessageRole messageRole,
        string messageContent,
        int sequenceNumber,
        DateTimeOffset utcNow
    )
    {
        if (id.IsEmpty)
            return MessageFaults.MessageIdRequired;
        
        if (chatId.IsEmpty)
            return MessageFaults.ChatIdRequired;

        if (!Enum.IsDefined(messageRole))
            return MessageFaults.InvalidMessageRole;

        if (string.IsNullOrWhiteSpace(messageContent))
            return MessageFaults.MessageContentRequired;

        if (sequenceNumber < 0)
            return MessageFaults.InvalidSequenceNumber;
        
        Message message = new
        (
            id: id,
            chatId: chatId,
            messageRole: messageRole,
            messageContent: messageContent,
            sequenceNumber: sequenceNumber,
            utcNow: utcNow
        );

        return message;
    }
}