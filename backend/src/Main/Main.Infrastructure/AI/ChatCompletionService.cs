using System.ClientModel;
using System.Text;

using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Stream;
using Main.Domain.Constants;
using Main.Domain.Enums;

using Microsoft.Extensions.Logging;

using OpenAI;
using OpenAI.Chat;

using SharedKernel;
using SharedKernel.Application.Messaging;

using StackExchange.Redis;

namespace Main.Infrastructure.AI;

internal sealed class ChatCompletionService(
    OpenAIClient openAiClient,
    IModelRegistry modelRegistry,
    IStreamPublisher streamPublisher,
    IMessageBus messageBus,
    IChatLockService chatLockService,
    IDateTimeProvider dateTimeProvider,
    ILogger<ChatCompletionService> logger) : IChatCompletionService
{
    private static readonly string TitleSystemPrompt =
        $"Generate a concise title (max {ChatConstants.MaxTitleLength} characters) for this conversation. " +
        "Return ONLY the title text, no quotes, no explanation.";

    private static readonly TimeSpan StreamExpiration = TimeSpan.FromHours(1);

    public async Task<string> GetTitleAsync(string message, CancellationToken cancellationToken)
    {
        string defaultModelId = modelRegistry.GetDefaultModelId();
        ChatClient chatClient = openAiClient.GetChatClient(defaultModelId);

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

    public async Task StreamCompletionAsync(string chatId, string streamId, string modelId,
        IReadOnlyList<ChatCompletionMessage> messages, CancellationToken cancellationToken)
    {
        StringBuilder messageContent = new();

        string openRouterId = modelRegistry.GetOpenRouterModelId(modelId);
        ChatClient chatClient = openAiClient.GetChatClient(openRouterId);

        try
        {
            await streamPublisher.PublishStatusAsync
            (
                streamId: streamId,
                status: StreamStatus.Pending,
                cancellationToken: cancellationToken
            );
            await streamPublisher.SetStreamExpirationAsync
            (
                streamId: streamId,
                expiration: StreamExpiration,
                cancellationToken: cancellationToken
            );

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

            await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(chatMessages,
                               cancellationToken: cancellationToken))
            {
#pragma warning disable S3267
                foreach (ChatMessageContentPart? part in update.ContentUpdate)
#pragma warning restore S3267
                {
                    string? chunk = part.Text;

                    if (!string.IsNullOrWhiteSpace(chunk))
                    {
                        messageContent.Append(chunk);

                        await streamPublisher.PublishChunkAsync
                        (
                            streamId: streamId,
                            messageContent: chunk,
                            cancellationToken: cancellationToken
                        );
                    }
                }
            }

            await streamPublisher.PublishStatusAsync
            (
                streamId: streamId,
                status: StreamStatus.Done,
                cancellationToken: cancellationToken
            );

            AssistantMessageGenerated assistantMessageGenerated = new()
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.NewGuid(),
                ChatId = chatId,
                MessageContent = messageContent.ToString(),
            };

            await messageBus.PublishAsync(assistantMessageGenerated, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Streaming failed for chat {ChatId}", chatId);

            try
            {
                await streamPublisher.PublishStatusAsync
                (
                    streamId: streamId,
                    status: StreamStatus.Failed,
                    cancellationToken: cancellationToken,
                    fault: exception.Message
                );
            }
            catch (RedisException redisException)
            {
                logger.LogError(redisException, "Failed to publish faulted status for chat {ChatId}", chatId);
            }

            throw;
        }
        finally
        {
            try
            {
                await chatLockService.ReleaseLockAsync(chatId, CancellationToken.None);
            }
            catch (RedisException ex)
            {
                logger.LogError(ex, "Failed to release lock for chat {ChatId}", chatId);
            }
        }
    }

    public Task StreamCompletionAdvancedAsync(Guid userId, string chatId, string streamId, string modelId, IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}