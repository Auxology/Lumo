namespace Main.Api.Endpoints.Chats.SendMessage;

internal sealed record Request
(
    string ChatId,
    string Message,
    bool WebSearchEnabled = false
);