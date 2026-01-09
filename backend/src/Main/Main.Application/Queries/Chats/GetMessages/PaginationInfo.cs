namespace Main.Application.Queries.Chats.GetMessages;

public sealed record PaginationInfo
(
    int? NextCursor,
    bool HasMore,
    int Limit
);