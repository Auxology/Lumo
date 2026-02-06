namespace Main.Application.Commands.EphemeralChats.SendEphemeralMessage;

public sealed record SendEphemeralMessageResponse
(
    string EphemeralChatId,
    string StreamId,
    string MessageRole,
    string MessageContent,
    DateTimeOffset CreatedAt
);