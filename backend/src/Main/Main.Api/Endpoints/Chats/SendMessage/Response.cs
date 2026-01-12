namespace Main.Api.Endpoints.Chats.SendMessage;

internal sealed record Response
(
    string ChatId,
    string MessageId,
    string StreamId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);