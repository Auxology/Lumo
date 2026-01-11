namespace Main.Application.Commands.Chats.SendMessage;

public sealed record SendMessageResponse
(
    string ChatId,
    string MessageId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);