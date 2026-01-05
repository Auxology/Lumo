namespace Main.Application.Chats.Update;

public sealed record UpdateChatResponse
(
    Guid ChatId,
    string Title,
    bool IsArchived,
    DateTimeOffset UpdatedAt
);