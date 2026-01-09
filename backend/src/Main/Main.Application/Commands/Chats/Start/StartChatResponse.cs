namespace Main.Application.Commands.Chats.Start;

public sealed record StartChatResponse
(
    string ChatId,
    string ChatTitle,
    DateTimeOffset CreatedAt
);