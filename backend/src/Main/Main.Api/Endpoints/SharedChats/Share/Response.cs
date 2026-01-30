namespace Main.Api.Endpoints.SharedChats.Share;

internal sealed record Response
(
    string SharedChatId,
    string SourceChatId,
    string Title,
    string ModelId,
    DateTimeOffset SnapshotAt,
    DateTimeOffset CreatedAt
);