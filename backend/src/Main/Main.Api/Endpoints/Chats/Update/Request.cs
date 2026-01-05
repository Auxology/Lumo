namespace Main.Api.Endpoints.Chats.Update;

internal sealed record Request
(
    Guid ChatId,
    string? NewTitle,
    bool? IsArchived
);