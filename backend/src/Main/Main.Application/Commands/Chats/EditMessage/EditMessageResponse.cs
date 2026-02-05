namespace Main.Application.Commands.Chats.EditMessage;

public sealed record EditMessageResponse
(
    string ChatId,
    string MessageId,
    string StreamId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset EditedAt
);