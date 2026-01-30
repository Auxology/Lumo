namespace Main.Application.Commands.Chats.Start;

public sealed record StartChatResponse
(
    string ChatId,
    string StreamId,
    string ChatTitle,
    string? ModelId,
    DateTimeOffset CreatedAt
);