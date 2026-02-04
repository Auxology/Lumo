using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.SharedChats.GetSharedChatsByOriginal;

public sealed record GetSharedChatByOriginalQuery(string OriginalChatId) : IQuery<GetSharedChatByOriginalResponse>;