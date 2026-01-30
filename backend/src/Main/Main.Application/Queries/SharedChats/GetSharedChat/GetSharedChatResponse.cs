namespace Main.Application.Queries.SharedChats.GetSharedChat;

public sealed record GetSharedChatResponse
(
    SharedChatReadModel SharedChat,
    IReadOnlyList<SharedChatMessageReadModel> Messages
);