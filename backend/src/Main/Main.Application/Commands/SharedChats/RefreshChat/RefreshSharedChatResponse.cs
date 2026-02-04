namespace Main.Application.Commands.SharedChats.RefreshChat;

public sealed record RefreshSharedChatResponse
(
    string SharedChatId,
    DateTimeOffset SnapshotAt,
    DateTimeOffset CreatedAt
);