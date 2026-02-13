using Main.Application.Abstractions.AI;
using Main.Domain.Enums;

using OpenAI.Chat;

namespace Main.Infrastructure.AI.Helpers;

internal static class ChatMessageExtensions
{
    public static ChatMessage ConvertToChatMessage(ChatCompletionMessage message) =>
        message.Role switch
        {
            MessageRole.User => new UserChatMessage(message.Content),
            MessageRole.Assistant => new AssistantChatMessage(message.Content),
            MessageRole.System => new SystemChatMessage(message.Content),
            _ => new UserChatMessage(message.Content)
        };
}