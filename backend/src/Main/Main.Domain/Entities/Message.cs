using System.Diagnostics.CodeAnalysis;
using Main.Domain.Enums;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;
using SharedKernel;

namespace Main.Domain.Entities;

public sealed class Message : Entity<int>
{
    public ChatId ChatId { get; private set; }

    public MessageRole MessageRole { get; private set; }

    public string MessageContent { get; private set; } = string.Empty;

    public long TokenCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Message() { } // For EF Core

    [SetsRequiredMembers]
    private Message
    (
        ChatId chatId,
        MessageRole messageRole,
        string messageContent,
        long tokenCount,
        DateTimeOffset utcNow
    )
    {
        ChatId = chatId;
        MessageRole = messageRole;
        MessageContent = messageContent;
        TokenCount = tokenCount;
        CreatedAt = utcNow;
    }

    public static Outcome<Message> Create
    (
        ChatId chatId,
        MessageRole messageRole,
        string messageContent,
        long tokenCount,
        DateTimeOffset utcNow
    )
    {
        if (chatId.IsEmpty)
            return MessageFaults.ChatIdRequired;

        if (!Enum.IsDefined(messageRole))
            return MessageFaults.InvalidMessageRole;

        if (string.IsNullOrWhiteSpace(messageContent))
            return MessageFaults.MessageContentRequired;

        if (tokenCount < 0)
            return MessageFaults.NegativeTokenCount;

        Message message = new
        (
            chatId: chatId,
            messageRole: messageRole,
            messageContent: messageContent,
            tokenCount: tokenCount,
            utcNow: utcNow
        );

        return message;
    }
}
