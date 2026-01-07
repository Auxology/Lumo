using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Start;

public sealed record StartChatCommand(string Message) : ICommand<StartChatResponse>;