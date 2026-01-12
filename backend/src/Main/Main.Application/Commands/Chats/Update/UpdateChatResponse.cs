namespace Main.Application.Commands.Chats.Update;

public sealed record UpdateChatResponse
(
    string ChatId,
    string Title,
    bool IsArchived,
    DateTimeOffset UpdatedAt
);