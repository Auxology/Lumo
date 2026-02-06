namespace Main.Api.Endpoints.EphemeralChats.Start;

internal sealed record Request
(
    string Message,
    string? ModelId = null
);