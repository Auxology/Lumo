namespace Main.Api.Endpoints.EphemeralChats.Start;

internal sealed record Response
(
    string EphemeralChatId,
    string StreamId
);