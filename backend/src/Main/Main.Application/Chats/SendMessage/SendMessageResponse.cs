namespace Main.Application.Chats.SendMessage;

public sealed record SendMessageResponse
(
    string ChatId,
    int MessageId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);