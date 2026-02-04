namespace Main.Api.Endpoints.Chats.Remix;

internal sealed record Response
(
    string ChatId,
    string StreamId,
    string ChatTitle,
    string ModelId,
    DateTimeOffset CreatedAt
);