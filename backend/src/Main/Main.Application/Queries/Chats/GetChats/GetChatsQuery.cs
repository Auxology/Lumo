using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Chats.GetChats;

public sealed record GetChatsQuery
(
    DateTimeOffset? Cursor,
    int Limit
) : IQuery<GetChatsResponse>;