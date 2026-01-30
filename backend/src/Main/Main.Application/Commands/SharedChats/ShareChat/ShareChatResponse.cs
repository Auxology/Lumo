namespace Main.Application.Commands.SharedChats.ShareChat;

public sealed record ShareChatResponse
(
    string SharedChatId,
    string SourceChatId,
    Guid OwnerId,
    string Title,
    string ModelId,
    DateTimeOffset SnapshotAt,
    DateTimeOffset CreatedAt
);