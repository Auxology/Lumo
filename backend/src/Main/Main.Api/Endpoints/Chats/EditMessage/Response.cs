namespace Main.Api.Endpoints.Chats.EditMessage;

internal sealed record Response
(
    string ChatId,
    string MessageId,
    string StreamId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset EditedAt
);