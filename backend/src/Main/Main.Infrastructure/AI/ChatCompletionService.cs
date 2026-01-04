using System.ClientModel;

using Main.Application.Abstractions.AI;
using Main.Domain.Constants;

using Microsoft.Extensions.Logging;

using OpenAI.Chat;

namespace Main.Infrastructure.AI;

internal sealed class ChatCompletionService(
    ChatClient chatClient,
    ILogger<ChatCompletionService> logger) : IChatCompletionService
{
    private static readonly string TitleSystemPrompt =
        $"Generate a concise title (max {ChatConstants.MaxTitleLength} characters) for this conversation. " +
        "Return ONLY the title text, no quotes, no explanation.";

    public async Task<string> GetTitleAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            List<ChatMessage> messages =
            [
                new SystemChatMessage(TitleSystemPrompt),
                new UserChatMessage(message)
            ];

            ChatCompletion response = await chatClient.CompleteChatAsync
            (
                messages,
                cancellationToken: cancellationToken
            );

            string? text = response.Content.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(text))
                return "New Chat";

            string title = text.Trim();

            if (title.Length > ChatConstants.MaxTitleLength)
                title = title[..(ChatConstants.MaxTitleLength - 3)] + "...";

            return title;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Failed to generate chat title due to network error, using fallback");
            return "New Chat";
        }
        catch (ClientResultException exception)
        {
            logger.LogWarning(exception, "Failed to generate chat title due to API error, using fallback");
            return "New Chat";
        }
    }

    public Task StreamCompletionAsync(Guid chatId, IReadOnlyList<ChatCompletionMessage> messages, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}