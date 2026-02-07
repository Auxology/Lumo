using System.Text;

using Contracts.IntegrationEvents.Chat;
using Contracts.IntegrationEvents.EphemeralChat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Instructions;
using Main.Application.Abstractions.Memory;
using Main.Application.Abstractions.Stream;
using Main.Domain.Enums;
using Main.Infrastructure.AI.Helpers;
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
    IInstructionStore instructionStore,
    IMemoryStore memoryStore,
    ToolExecutor toolExecutor,
    IDateTimeProvider dateTimeProvider,
    ILogger<ChatCompletionService> logger) : IChatCompletionService
{
    private static readonly TimeSpan StreamExpiration = TimeSpan.FromHours(1);
    private const int MaxToolCallIterations = 5;

    public Task StreamCompletionAsync
    (
        string chatId,
        string streamId,
        string modelId,
        string correlationId,
        IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken
        )
    {
        return ExecuteStreamingAsync
        (
            chatId: chatId,
            streamId: streamId,
            modelId: modelId,
            messages: messages,
            correlationId: correlationId,
            toolContext: null,
            cancellationToken: cancellationToken
        );
    }

    public Task StreamCompletionAdvancedAsync
    (
        Guid userId,
        string chatId,
        string streamId,
        string modelId,
        string correlationId,
        IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken)
    {
        return ExecuteStreamingAsync
        (
            chatId: chatId,
            streamId: streamId,
            modelId: modelId,
            messages: messages,
            correlationId: correlationId,
            toolContext: new ToolContext(userId),
            cancellationToken: cancellationToken
        );
    }

    private async Task ExecuteStreamingAsync
    (
        string chatId,
        string streamId,
        string modelId,
        string correlationId,
        IReadOnlyList<ChatCompletionMessage> messages,
        ToolContext? toolContext,
        CancellationToken cancellationToken
    )
    {
        StringBuilder messageContent = new();

        try
        {
            await InitializeStreamAsync(streamId, cancellationToken);

            ChatClient chatClient = GetChatClient(modelId);

            List<ChatMessage> chatMessages = await BuildChatMessagesAsync(messages, toolContext, modelId, cancellationToken);

            ChatCompletionOptions? options = toolContext is not null
                ? new ChatCompletionOptions { Tools = { ToolDefinitions.SaveMemory } }
                : null;

            await ProcessStreamingResponseAsync
            (
                chatClient: chatClient,
                chatMessages: chatMessages,
                options: options,
                streamId: streamId,
                chatId: chatId,
                messageContent: messageContent,
                toolContext: toolContext,
                cancellationToken: cancellationToken
            );

            await FinalizeStreamAsync
            (
                streamId: streamId,
                chatId: chatId,
                messageContent: messageContent,
                toolContext: toolContext,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception exception)
        {
            await HandleStreamingErrorAsync(chatId, streamId, exception, cancellationToken);
            throw;
        }
        finally
        {
            await ReleaseChatLockAsync(chatId, correlationId);
        }
    }

    private async Task ProcessStreamingResponseAsync
    (
        ChatClient chatClient,
        List<ChatMessage> chatMessages,
        ChatCompletionOptions? options,
        string streamId,
        string chatId,
        StringBuilder messageContent,
        ToolContext? toolContext,
        CancellationToken cancellationToken
    )
    {
        int iterations = 0;

        while (iterations < MaxToolCallIterations)
        {
            iterations++;

            StreamIterationResult result = await ProcessSingleIterationAsync
            (
                chatClient: chatClient,
                chatMessages: chatMessages,
                options: options,
                streamId: streamId,
                chatId: chatId,
                messageContent: messageContent,
                cancellationToken: cancellationToken
            );

            if (result.HasProviderError)
                throw new InvalidOperationException("The AI provider returned an error. Please try again or select a different model.");

            if (result.ToolCalls.Count == 0)
                break;

            if (toolContext is null)
                break;

            await ProcessToolCallsAsync(chatMessages, result, toolContext.UserId, cancellationToken);
        }

        if (messageContent.Length == 0 && iterations > 0 && toolContext is not null)
            await StreamFinalResponseAsync(chatClient, chatMessages, streamId, messageContent, cancellationToken);
    }

    private async Task<StreamIterationResult> ProcessSingleIterationAsync
    (
        ChatClient chatClient,
        List<ChatMessage> chatMessages,
        ChatCompletionOptions? options,
        string streamId,
        string chatId,
        StringBuilder messageContent,
        CancellationToken cancellationToken
    )
    {
        ToolCallAccumulator toolAccumulator = new();
        StringBuilder contentBuilder = new();
        bool providerError = false;

        try
        {
            IAsyncEnumerable<StreamingChatCompletionUpdate> stream = options is not null
                ? chatClient.CompleteChatStreamingAsync(chatMessages, options, cancellationToken)
                : chatClient.CompleteChatStreamingAsync(chatMessages, cancellationToken: cancellationToken);

            await foreach (StreamingChatCompletionUpdate update in stream)
            {
                toolAccumulator.ProcessUpdate(update);
                await PublishContentChunksAsync
                (
                    update: update,
                    streamId: streamId,
                    messageContent: messageContent,
                    iterationContent: contentBuilder,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (ArgumentOutOfRangeException exception) when (exception.Message.Contains("ChatFinishReason", StringComparison.Ordinal))
        {
            logger.LogWarning(exception, "Provider returned unknown finish reason for chat {ChatId}", chatId);
            providerError = true;
        }

        return new StreamIterationResult
        (
            ToolCalls: toolAccumulator.Build(),
            Content: contentBuilder.ToString(),
            HasProviderError: providerError
        );
    }

    private async Task PublishContentChunksAsync
    (
        StreamingChatCompletionUpdate update,
        string streamId,
        StringBuilder messageContent,
        StringBuilder? iterationContent,
        CancellationToken cancellationToken
    )
    {
#pragma warning disable S3267
        foreach (ChatMessageContentPart? part in update.ContentUpdate)
#pragma warning restore S3267
        {
            string? chunk = part.Text;

            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            iterationContent?.Append(chunk);
            messageContent.Append(chunk);

            await streamPublisher.PublishChunkAsync(streamId, chunk, cancellationToken);
        }
    }

    private async Task ProcessToolCallsAsync
    (
        List<ChatMessage> chatMessages,
        StreamIterationResult result,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        AssistantChatMessage assistantMessage = new(result.ToolCalls);

        if (result.Content.Length > 0)
            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(result.Content));

        chatMessages.Add(assistantMessage);

        foreach (ChatToolCall toolCall in result.ToolCalls)
        {
            string toolResult = await toolExecutor.ExecuteAsync(toolCall, userId, cancellationToken);
            chatMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
        }
    }

    private async Task StreamFinalResponseAsync
    (
        ChatClient chatClient,
        List<ChatMessage> chatMessages,
        string streamId,
        StringBuilder messageContent,
        CancellationToken cancellationToken
    )
    {
        await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(
                           chatMessages, cancellationToken: cancellationToken))
        {
            await PublishContentChunksAsync
            (
                update: update,
                streamId: streamId,
                messageContent: messageContent,
                iterationContent: null,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task<List<ChatMessage>> BuildChatMessagesAsync
    (
        IReadOnlyList<ChatCompletionMessage> messages,
        ToolContext? toolContext,
        string modelId,
        CancellationToken cancellationToken
    )
    {
        List<ChatMessage> chatMessages = [];

        if (toolContext is not null)
        {
            string latestUserMessage = messages
                .Where(m => m.Role == MessageRole.User)
                .Select(m => m.Content)
                .LastOrDefault() ?? string.Empty;

            IReadOnlyList<MemoryEntry> memories = await memoryStore.GetRelevantAsync
            (
                toolContext.UserId,
                latestUserMessage,
                MemoryConstants.MaxMemoriesInContext,
                cancellationToken
            );

            IReadOnlyList<InstructionEntry> instructions = await instructionStore
                .GetForUserAsync(toolContext.UserId, cancellationToken);

            ModelInfo? modelInfo = modelRegistry.GetModelInfo(modelId);

            chatMessages.Add(
                new SystemChatMessage(SystemPromptBuilder.Build(instructions, memories, modelInfo, dateTimeProvider)));
        }

        chatMessages.AddRange(messages.Select(ChatMessageExtensions.ConvertToChatMessage));
        return chatMessages;
    }

    private ChatClient GetChatClient(string modelId)
    {
        string openRouterId = modelRegistry.GetOpenRouterModelId(modelId);
        return openAiClient.GetChatClient(openRouterId);
    }

    private async Task InitializeStreamAsync(string streamId, CancellationToken cancellationToken)
    {
        await streamPublisher.PublishStatusAsync(streamId, StreamStatus.Pending, cancellationToken);
        await streamPublisher.SetStreamExpirationAsync(streamId, StreamExpiration, cancellationToken);
    }

    private async Task FinalizeStreamAsync
    (
        string streamId,
        string chatId,
        StringBuilder messageContent,
        ToolContext? toolContext,
        CancellationToken cancellationToken
    )
    {
        await streamPublisher.PublishStatusAsync(streamId, StreamStatus.Done, cancellationToken);

        if (messageContent.Length == 0)
            return;

        string content = messageContent.ToString();

        if (toolContext is not null)
        {
            await messageBus.PublishAsync(new AssistantMessageGenerated
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.NewGuid(),
                ChatId = chatId,
                MessageContent = content
            }, cancellationToken);
        }
        else
        {
            await messageBus.PublishAsync(new AssistantEphemeralMessageGenerated
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.NewGuid(),
                EphemeralChatId = chatId,
                MessageContent = content
            }, cancellationToken);
        }
    }

    private async Task HandleStreamingErrorAsync
    (
        string chatId,
        string streamId,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "Streaming failed for chat {ChatId}", chatId);

        try
        {
            await streamPublisher.PublishStatusAsync(streamId, StreamStatus.Failed, cancellationToken, exception.Message);
        }
        catch (RedisException redisException)
        {
            logger.LogError(redisException, "Failed to publish faulted status for chat {ChatId}", chatId);
        }
    }

    private async Task ReleaseChatLockAsync(string chatId, string ownerId)
    {
        try
        {
            await chatLockService.ReleaseLockAsync
            (
                chatId: chatId,
                ownerId: ownerId,
                cancellationToken: CancellationToken.None
            );
        }
        catch (RedisException exception)
        {
            logger.LogError(exception, "Failed to release lock for chat {ChatId}", chatId);
        }
    }

    private sealed record ToolContext(Guid UserId);
}