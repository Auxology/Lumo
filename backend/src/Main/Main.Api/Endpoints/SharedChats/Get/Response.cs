namespace Main.Api.Endpoints.SharedChats.Get;

internal sealed record SharedChatDto
(
    string Id,
    string SourceChatId,
    Guid OwnerId,
    string Title,
    string ModelId,
    int ViewCount,
    DateTimeOffset SnapshotAt,
    DateTimeOffset CreatedAt
);

internal sealed record SharedChatMessageDto
(
    int SequenceNumber,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);

internal sealed record Response
(
    SharedChatDto SharedChat,
    IReadOnlyList<SharedChatMessageDto> Messages
);