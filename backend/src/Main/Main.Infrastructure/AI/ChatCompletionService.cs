using System.ClientModel;
using System.Text;

using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.AI;
using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.Enums;

using Microsoft.Extensions.Logging;

using OpenAI.Chat;

using SharedKernel;
using SharedKernel.Application.Messaging;

namespace Main.Infrastructure.AI;

internal sealed class ChatCompletionService(
    ChatClient chatClient,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider,
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

    public async Task StreamCompletionAsync(Guid chatId, IReadOnlyList<ChatCompletionMessage> messages, CancellationToken cancellationToken)
    {
        StringBuilder messageContent = new();

        try
        {
            List<ChatMessage> chatMessages = messages.Select<ChatCompletionMessage, ChatMessage>(m =>
                {
                    return m.Role switch
                    {
                        MessageRole.User => new UserChatMessage(m.Content),
                        MessageRole.Assistant => new AssistantChatMessage(m.Content),
                        MessageRole.System => new SystemChatMessage(m.Content),
                        _ => new UserChatMessage(m.Content)
                    };
                })
                .ToList();

#pragma warning disable S3267
            await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(
                               chatMessages, cancellationToken: cancellationToken))
#pragma warning restore S3267
            {
                if (update.ContentUpdate.Count > 0)
                {
                    string chunk = update.ContentUpdate[0].Text;
                    messageContent.Append(chunk);
                }
                else
                {
                    logger.LogWarning("Received empty content update for chat {ChatId}", chatId);
                }
            }
            
            AssistantMessageGenerated assistantMessageGenerated = new()
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = chatId,
                ChatId = chatId,
                MessageContent = messageContent.ToString(),
            };

            await messageBus.PublishAsync(assistantMessageGenerated, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Streaming failed for chat {ChatId}", chatId);

            throw;
        }
    }
}