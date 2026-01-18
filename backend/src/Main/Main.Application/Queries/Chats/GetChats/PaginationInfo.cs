namespace Main.Application.Queries.Chats.GetChats;

public sealed record PaginationInfo
(
    DateTimeOffset? NextCursor,
    bool HasMore,
    int Limit
);