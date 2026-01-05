using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Delete;

public sealed record DeleteChatCommand(Guid ChatId) : ICommand;