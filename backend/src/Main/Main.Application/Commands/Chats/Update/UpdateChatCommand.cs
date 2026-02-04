using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.Update;

public sealed record UpdateChatCommand
(
    string ChatId,
    string? NewTitle,
    bool? IsArchived,
    bool? IsPinned
) : ICommand<UpdateChatResponse>;