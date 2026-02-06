namespace Main.Application.Commands.EphemeralChats.Start;

public record StartEphemeralChatResponse
(
    string EphemeralChatId,
    string StreamId
);