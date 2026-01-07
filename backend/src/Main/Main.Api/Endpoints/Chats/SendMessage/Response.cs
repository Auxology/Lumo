namespace Main.Api.Endpoints.Chats.SendMessage;

internal sealed record Response
(
    string ChatId,
    int MessageId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);