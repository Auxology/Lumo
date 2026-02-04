using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.SharedChats.DeleteSharedChat;

public sealed record DeleteSharedChatCommand(string SharedChatId) : ICommand;