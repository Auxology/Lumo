namespace Main.Application.Queries.Chats.GetMessages;

public sealed record GetMessagesResponse
(
    IReadOnlyList<MessageReadModel> Messages,
    PaginationInfo Pagination
);