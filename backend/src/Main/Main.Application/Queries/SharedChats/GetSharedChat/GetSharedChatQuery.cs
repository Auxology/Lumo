using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.SharedChats.GetSharedChat;

public sealed record GetSharedChatQuery(string SharedChatId) : IQuery<GetSharedChatResponse>;