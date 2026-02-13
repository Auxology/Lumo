using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.Remix;

public sealed record RemixChatCommand(string ChatId, string NewModelId, bool WebSearchEnabled) : ICommand<RemixChatResponse>;