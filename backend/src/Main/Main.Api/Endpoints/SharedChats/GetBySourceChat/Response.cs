namespace Main.Api.Endpoints.SharedChats.GetBySourceChat;

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

internal sealed record Response
(
    IReadOnlyList<SharedChatDto> SharedChats
);