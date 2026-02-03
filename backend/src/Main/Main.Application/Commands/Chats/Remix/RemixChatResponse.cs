namespace Main.Application.Commands.Chats.Remix;

public sealed record RemixChatResponse
(
    string ChatId,
    string StreamId,
    string ChatTitle,
    string ModelId,
    DateTimeOffset CreatedAt
);