using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.SharedChats.ForkSharedChat;

public sealed record ForkSharedChatCommand(string SharedChatId) : ICommand<ForkSharedChatResponse>;