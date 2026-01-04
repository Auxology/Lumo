namespace Main.Application.Chats.Starts;

public sealed record StartChatResponse
(
    Guid ChatId,
    string ChatTitle,
    DateTimeOffset CreatedAt
);