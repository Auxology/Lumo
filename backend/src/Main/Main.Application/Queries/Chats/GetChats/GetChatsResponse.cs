namespace Main.Application.Queries.Chats.GetChats;

public sealed record GetChatsResponse
(
    IReadOnlyList<ChatReadModel> Chats,
    PaginationInfo PaginationInfo
);