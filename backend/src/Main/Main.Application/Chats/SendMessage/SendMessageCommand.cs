using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.SendMessage;

public sealed record SendMessageCommand
(
    string ChatId,
    string Message
) : ICommand<SendMessageResponse>;