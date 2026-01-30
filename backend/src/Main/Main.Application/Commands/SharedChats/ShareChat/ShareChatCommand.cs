using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.SharedChats.ShareChat;

public sealed record class ShareChatCommand(string ChatId) : ICommand<ShareChatResponse>;