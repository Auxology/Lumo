using Main.Application.Queries.Chats.GetMessages;

namespace Main.Api.Endpoints.Chats.GetMessages;

internal sealed record MessageDto
(
    int Id,
    string ChatId,
    string MessageRole,
    string MessageContent,
    long? TokenCount,
    DateTimeOffset CreatedAt
);

internal sealed record PaginationDto
(
    int? NextCursor,
    bool HasMore,
    int Limit
);

internal sealed record Response(
    IReadOnlyList<MessageDto> Messages,
    PaginationDto Pagination
);