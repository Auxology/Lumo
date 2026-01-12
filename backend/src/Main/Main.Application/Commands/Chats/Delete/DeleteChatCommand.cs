using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.Delete;

public sealed record DeleteChatCommand(string ChatId) : ICommand;