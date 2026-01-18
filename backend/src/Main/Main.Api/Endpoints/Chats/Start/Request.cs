namespace Main.Api.Endpoints.Chats.Start;

internal sealed record Request
(
    string Message,
    string? ModelId = null
);