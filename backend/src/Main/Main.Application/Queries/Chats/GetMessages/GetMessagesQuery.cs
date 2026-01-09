using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Chats.GetMessages;

public sealed record GetMessagesQuery
(
    string ChatId,
    int? Cursor,
    int Limit
) : IQuery<GetMessagesResponse>;