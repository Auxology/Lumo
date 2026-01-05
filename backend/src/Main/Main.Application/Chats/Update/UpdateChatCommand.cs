using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Update;

public sealed record UpdateChatCommand
(
    Guid ChatId,
    string? NewTitle,
    bool? IsArchived
) : ICommand<UpdateChatResponse>;