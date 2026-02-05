using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.EditMessage;

public sealed record EditMessageCommand
(
    string ChatId,
    string MessageId,
    string NewContent
) : ICommand<EditMessageResponse>;