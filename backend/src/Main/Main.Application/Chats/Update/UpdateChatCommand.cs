using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Update;

public sealed record UpdateChatCommand
(
    string ChatId,
    string? NewTitle,
    bool? IsArchived
) : ICommand<UpdateChatResponse>;