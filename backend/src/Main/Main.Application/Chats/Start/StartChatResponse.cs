namespace Main.Application.Chats.Start;

public sealed record StartChatResponse
(
    string ChatId,
    string ChatTitle,
    DateTimeOffset CreatedAt
);