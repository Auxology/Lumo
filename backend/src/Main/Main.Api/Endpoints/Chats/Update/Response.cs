namespace Main.Api.Endpoints.Chats.Update;

internal sealed record Response
(
    Guid ChatId,
    string Title,
    bool IsArchived,
    DateTimeOffset UpdatedAt
);