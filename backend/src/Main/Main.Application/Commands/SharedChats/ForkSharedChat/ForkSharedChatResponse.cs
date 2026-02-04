namespace Main.Application.Commands.SharedChats.ForkSharedChat;

public sealed record ForkSharedChatResponse
(
    string ChatId,
    string Title,
    string ModelId,
    DateTimeOffset CreatedAt
);