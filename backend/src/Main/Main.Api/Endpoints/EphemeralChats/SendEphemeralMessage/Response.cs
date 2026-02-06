namespace Main.Api.Endpoints.EphemeralChats.SendEphemeralMessage;

internal sealed record Response
(
    string EphemeralChatId,
    string StreamId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);