using System.ClientModel;

using Main.Application.Abstractions.AI;
using Main.Domain.Constants;

using Microsoft.Extensions.Logging;

using OpenAI;
using OpenAI.Chat;

namespace Main.Infrastructure.AI;

internal sealed class TitleGenerator
(
    OpenAIClient openAiClient,
    IModelRegistry modelRegistry,
    ILogger<TitleGenerator> logger
) : ITitleGenerator
{
    private static readonly string TitleSystemPrompt =
        $"Generate a concise title (max {ChatConstants.MaxTitleLength} characters) for this conversation. " +
        "Return ONLY the title text, no quotes, no explanation.";

    public async Task<string> GetTitleAsync(string message, CancellationToken cancellationToken)
    {
        ChatClient chatClient = GetChatClient(modelRegistry.GetDefaultModelId());

        try
        {
            List<ChatMessage> messages =
            [
                new SystemChatMessage(TitleSystemPrompt),
                new UserChatMessage(message)
            ];

            ChatCompletion response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            string? text = response.Content.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(text))
                return "New Chat";

            string title = text.Trim();
            return title.Length > ChatConstants.MaxTitleLength
                ? title[..(ChatConstants.MaxTitleLength - 3)] + "..."
                : title;
        }
        catch (Exception ex) when (ex is HttpRequestException or ClientResultException)
        {
            logger.LogWarning(ex, "Failed to generate chat title, using fallback");
            return "New Chat";
        }
    }

    private ChatClient GetChatClient(string modelId)
    {
        string openRouterId = modelRegistry.GetOpenRouterModelId(modelId);
        return openAiClient.GetChatClient(openRouterId);
    }
}