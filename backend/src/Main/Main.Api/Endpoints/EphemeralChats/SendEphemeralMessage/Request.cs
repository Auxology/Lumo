namespace Main.Api.Endpoints.EphemeralChats.SendEphemeralMessage;

internal sealed record Request
(
    string EphemeralChatId,
    string Message
);