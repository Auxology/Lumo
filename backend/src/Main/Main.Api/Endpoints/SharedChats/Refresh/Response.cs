namespace Main.Api.Endpoints.SharedChats.Refresh;

internal sealed record Response
(
    string SharedChatId,
    DateTimeOffset SnapshotAt,
    DateTimeOffset CreatedAt
);