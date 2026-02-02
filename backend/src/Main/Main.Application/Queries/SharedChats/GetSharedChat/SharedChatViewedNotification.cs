using Mediator;

namespace Main.Application.Queries.SharedChats.GetSharedChat;

public sealed record SharedChatViewedNotification(string SharedChatId) : INotification;