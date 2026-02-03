namespace Main.Api.Endpoints.Chats.Remix;

internal sealed record Request
(
    string ChatId,
    string NewModelId
);