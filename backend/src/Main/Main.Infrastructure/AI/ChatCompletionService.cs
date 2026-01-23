using System.ClientModel;
using System.Globalization;
using System.Text;

using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Memory;
using Main.Application.Abstractions.Stream;
using Main.Domain.Constants;
using Main.Domain.Enums;
using Main.Infrastructure.AI.Tools;

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
    IMemoryStore memoryStore,
    ToolExecutor toolExecutor,
    IDateTimeProvider dateTimeProvider,
    ILogger<ChatCompletionService> logger) : IChatCompletionService
{
    private static readonly string TitleSystemPrompt =
        $"Generate a concise title (max {ChatConstants.MaxTitleLength} characters) for this conversation. " +
        "Return ONLY the title text, no quotes, no explanation.";

    private static readonly TimeSpan StreamExpiration = TimeSpan.FromHours(1);
    private const int MaxToolCallIterations = 5;

    public async Task<string> GetTitleAsync(string message, CancellationToken cancellationToken)
    {
        string defaultModelId = modelRegistry.GetDefaultModelId();
        string defaultOpenRouterId = modelRegistry.GetOpenRouterModelId(defaultModelId);
        ChatClient chatClient = openAiClient.GetChatClient(defaultOpenRouterId);

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

        try
        {
            string openRouterId = modelRegistry.GetOpenRouterModelId(modelId);
            ChatClient chatClient = openAiClient.GetChatClient(openRouterId);

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

    public async Task StreamCompletionAdvancedAsync(Guid userId, string chatId, string streamId, string modelId, IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken)
    {
        StringBuilder messageContent = new();

        try
        {
            string openRouterId = modelRegistry.GetOpenRouterModelId(modelId);
            ChatClient chatClient = openAiClient.GetChatClient(openRouterId);

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

            string latestUserMessage = messages
                .Where(m => m.Role == MessageRole.User)
                .Select(m => m.Content)
                .LastOrDefault() ?? string.Empty;

            IReadOnlyList<MemoryEntry> memories = await memoryStore.GetRelevantAsync
            (
                userId: userId,
                context: latestUserMessage,
                limit: MemoryConstants.MaxMemoriesInContext,
                cancellationToken: cancellationToken
            );

            string systemPrompt = BuildSystemPromptWithMemories(memories);

            List<ChatMessage> chatMessages = [new SystemChatMessage(systemPrompt)];
            chatMessages.AddRange(messages.Select<ChatCompletionMessage, ChatMessage>(m =>
            {
                return m.Role switch
                {
                    MessageRole.User => new UserChatMessage(m.Content),
                    MessageRole.Assistant => new AssistantChatMessage(m.Content),
                    MessageRole.System => new SystemChatMessage(m.Content),
                    _ => new UserChatMessage(m.Content)
                };
            }));

            ChatCompletionOptions options = new()
            {
                Tools = { ToolDefinitions.SaveMemory }
            };

            int iterations = 0;

            while (iterations < MaxToolCallIterations)
            {
                iterations++;

                List<ChatToolCall> accumulatedToolCalls = new();
                Dictionary<int, StringBuilder> chatToolCallArguments = new();
                Dictionary<int, string> chatToolCallIds = new();
                Dictionary<int, string> chatToolCallNames = new();
                StringBuilder contentBuilder = new();
                ChatFinishReason? finishReason = null;

                await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(
                                   chatMessages, options, cancellationToken))
                {
                    foreach (StreamingChatToolCallUpdate chatToolCallUpdate in update.ToolCallUpdates)
                    {
                        int index = chatToolCallUpdate.Index;

                        if (!chatToolCallArguments.ContainsKey(index))
                            chatToolCallArguments[index] = new StringBuilder();

                        if (!string.IsNullOrWhiteSpace(chatToolCallUpdate.FunctionArgumentsUpdate?.ToString()))
                            chatToolCallArguments[index].Append(chatToolCallUpdate.FunctionArgumentsUpdate);

                        if (chatToolCallUpdate.ToolCallId is not null)
                            chatToolCallIds[index] = chatToolCallUpdate.ToolCallId;

                        if (chatToolCallUpdate.FunctionName is not null)
                            chatToolCallNames[index] = chatToolCallUpdate.FunctionName;
                    }

#pragma warning disable S3267
                    foreach (ChatMessageContentPart? part in update.ContentUpdate)
#pragma warning restore S3267
                    {
                        string? chunk = part.Text;

                        if (!string.IsNullOrWhiteSpace(chunk))
                        {
                            contentBuilder.Append(chunk);
                            messageContent.Append(chunk);

                            await streamPublisher.PublishChunkAsync
                            (
                                streamId: streamId,
                                messageContent: chunk,
                                cancellationToken: cancellationToken
                            );
                        }
                    }

                    finishReason = update.FinishReason;
                }

                foreach (int index in chatToolCallIds.Keys)
                {
                    if (chatToolCallNames.TryGetValue(index, out string? name) &&
                        chatToolCallArguments.TryGetValue(index, out StringBuilder? arguments))
                    {
                        accumulatedToolCalls.Add(ChatToolCall.CreateFunctionToolCall
                        (
                            id: chatToolCallIds[index],
                            functionName: name,
                            functionArguments: BinaryData.FromString(arguments.ToString())
                        ));
                    }
                }

                if (accumulatedToolCalls.Count == 0 || finishReason != ChatFinishReason.ToolCalls)
                    break;

                AssistantChatMessage assistantChatMessage = new(accumulatedToolCalls);

                if (contentBuilder.Length > 0)
                    assistantChatMessage.Content.Add(ChatMessageContentPart.CreateTextPart(contentBuilder.ToString()));

                chatMessages.Add(assistantChatMessage);

                foreach (ChatToolCall chatToolCall in accumulatedToolCalls)
                {
                    logger.LogInformation("Executing tool call {ToolCallId} ({FunctionName}) for chat {ChatId}",
                        chatToolCall.Id, chatToolCall.FunctionName, chatId);

                    string result = await toolExecutor.ExecuteAsync
                    (
                        chatToolCall: chatToolCall,
                        userId: userId,
                        cancellationToken: cancellationToken
                    );

                    chatMessages.Add(new ToolChatMessage(chatToolCall.Id, result));
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

    private static string BuildSystemPromptWithMemories(IReadOnlyList<MemoryEntry> memories)
    {
        if (memories.Count == 0)
        {
            return "You are a helpful AI assistant. " +
                   "If the user shares important information about themselves (preferences, facts, or instructions), " +
                   "use the save_memory function to remember it for future conversations.";
        }

        StringBuilder sb = new();
        sb.AppendLine("You are a helpful AI assistant with access to the user's saved memories.");
        sb.AppendLine();
        sb.AppendLine("Relevant user memories:");
        foreach (MemoryEntry memory in memories)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"- [{memory.MemoryCategory}] {memory.Content}");
        }
        sb.AppendLine();
        sb.AppendLine("Use these memories to personalize your responses. " +
                      "If the user shares new important information, use the save_memory function to remember it.");

        return sb.ToString();
    }
}