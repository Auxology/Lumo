using Main.Domain.Enums;

namespace Main.Domain.ValueObjects;

public sealed record SharedChatMessage
(
    int SequenceNumber,
    MessageRole MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt,
    DateTimeOffset EditedAt
);