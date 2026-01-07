namespace Main.Api.Endpoints.Chats.Update;

internal sealed record Response
(
    string ChatId,
    string Title,
    bool IsArchived,
    DateTimeOffset UpdatedAt
);