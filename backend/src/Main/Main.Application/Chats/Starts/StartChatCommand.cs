using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Starts;

public sealed record StartChatCommand(string Message) : ICommand<StartChatResponse>;
