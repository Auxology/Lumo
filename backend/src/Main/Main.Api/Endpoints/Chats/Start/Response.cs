namespace Main.Api.Endpoints.Chats.Start;

internal sealed record Response
(
    Guid ChatId,
    string ChatTitle,
    DateTimeOffset CreatedAt
);