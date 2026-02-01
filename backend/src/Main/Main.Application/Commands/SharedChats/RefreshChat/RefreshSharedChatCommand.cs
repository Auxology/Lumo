using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.SharedChats.RefreshChat;

public sealed record RefreshSharedChatCommand(string SharedChatId) : ICommand<RefreshSharedChatResponse>;