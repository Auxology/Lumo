namespace Main.Api.Endpoints.Chats.SendMessage;

internal sealed record Response
(
    string ChatId,
    string MessageId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);