namespace Main.Api.Endpoints.Chats.Start;

internal sealed record Response
(
    string ChatId,
    string ChatTitle,
    string StreamId,
    DateTimeOffset CreatedAt
);