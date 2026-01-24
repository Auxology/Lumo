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

    public Task StreamCompletionAsync
    (
        string chatId,
        string streamId,
        string modelId,
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
        IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken)
    {
        return ExecuteStreamingAsync
        (
            chatId: chatId,
            streamId: streamId,
            modelId: modelId,
            messages: messages,
            toolContext: new ToolContext(userId),
            cancellationToken: cancellationToken
        );
    }

    private async Task ExecuteStreamingAsync
    (
        string chatId,
        string streamId,
        string modelId,
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
            
            List<ChatMessage> chatMessages = await BuildChatMessagesAsync(messages, toolContext, cancellationToken);
            
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

            await FinalizeStreamAsync(streamId, chatId, messageContent, cancellationToken);
        }
        catch (Exception exception)
        {
            await HandleStreamingErrorAsync(chatId, streamId, exception, cancellationToken);
            throw;
        }
        finally
        {
            await ReleaseChatLockAsync(chatId);
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
                await ProcessContentUpdateAsync(update, streamId, messageContent, contentBuilder, cancellationToken);
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

    private async Task ProcessContentUpdateAsync
    (
        StreamingChatCompletionUpdate update,
        string streamId,
        StringBuilder messageContent,
        StringBuilder contentBuilder,
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

            contentBuilder.Append(chunk);
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
#pragma warning disable S3267
            foreach (ChatMessageContentPart? part in update.ContentUpdate)
#pragma warning restore S3267
            {
                string? chunk = part.Text;

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    messageContent.Append(chunk);
                    await streamPublisher.PublishChunkAsync(streamId, chunk, cancellationToken);
                }
            }
        }
    }

    private async Task<List<ChatMessage>> BuildChatMessagesAsync
    (
        IReadOnlyList<ChatCompletionMessage> messages,
        ToolContext? toolContext,
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

            IReadOnlyList<MemoryEntry> memories = await memoryStore.GetRelevantAsync(
                toolContext.UserId,
                latestUserMessage,
                MemoryConstants.MaxMemoriesInContext,
                cancellationToken);

            chatMessages.Add(new SystemChatMessage(BuildSystemPrompt(memories)));
        }

        chatMessages.AddRange(messages.Select(ConvertToChatMessage));
        return chatMessages;
    }

    private static ChatMessage ConvertToChatMessage(ChatCompletionMessage message) =>
        message.Role switch
        {
            MessageRole.User => new UserChatMessage(message.Content),
            MessageRole.Assistant => new AssistantChatMessage(message.Content),
            MessageRole.System => new SystemChatMessage(message.Content),
            _ => new UserChatMessage(message.Content)
        };

    private static string BuildSystemPrompt(IReadOnlyList<MemoryEntry> memories)
    {
        const string baseInstruction = "When the user shares important information about themselves (preferences, facts, or instructions), " +
                                       "you MUST call the save_memory function to persist it. " +
                                       "Do NOT just say you will remember - actually invoke the function.";

        if (memories.Count == 0)
            return $"You are a helpful AI assistant. {baseInstruction}";

        StringBuilder sb = new();
        sb.AppendLine("You are a helpful AI assistant with access to the user's saved memories.");
        sb.AppendLine();
        sb.AppendLine("Relevant user memories:");

        foreach (MemoryEntry memory in memories)
            sb.AppendLine(CultureInfo.InvariantCulture, $"- [{memory.MemoryCategory}] {memory.Content}");

        sb.AppendLine();
        sb.AppendLine($"Use these memories to personalize your responses. {baseInstruction}");

        return sb.ToString();
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
        CancellationToken cancellationToken
    )
    {
        await streamPublisher.PublishStatusAsync(streamId, StreamStatus.Done, cancellationToken);

        if (messageContent.Length > 0)
        {
            await messageBus.PublishAsync(new AssistantMessageGenerated
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.NewGuid(),
                ChatId = chatId,
                MessageContent = messageContent.ToString()
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

    private async Task ReleaseChatLockAsync(string chatId)
    {
        try
        {
            await chatLockService.ReleaseLockAsync(chatId, CancellationToken.None);
        }
        catch (RedisException exception)
        {
            logger.LogError(exception, "Failed to release lock for chat {ChatId}", chatId);
        }
    }

    private sealed record ToolContext(Guid UserId);

    private sealed record StreamIterationResult
    (
        IReadOnlyList<ChatToolCall> ToolCalls,
        string Content,
        bool HasProviderError
    );

    private sealed class ToolCallAccumulator
    {
        private readonly Dictionary<int, StringBuilder> _arguments = [];
        private readonly Dictionary<int, string> _ids = [];
        private readonly Dictionary<int, string> _names = [];

        public void ProcessUpdate(StreamingChatCompletionUpdate update)
        {
            foreach (StreamingChatToolCallUpdate toolCallUpdate in update.ToolCallUpdates)
            {
                int index = toolCallUpdate.Index;

                _arguments.TryAdd(index, new StringBuilder());

                if (!string.IsNullOrWhiteSpace(toolCallUpdate.FunctionArgumentsUpdate?.ToString()))
                    _arguments[index].Append(toolCallUpdate.FunctionArgumentsUpdate);

                if (toolCallUpdate.ToolCallId is not null)
                    _ids[index] = toolCallUpdate.ToolCallId;

                if (toolCallUpdate.FunctionName is not null)
                    _names[index] = toolCallUpdate.FunctionName;
            }
        }

        public List<ChatToolCall> Build()
        {
            List<ChatToolCall> toolCalls = [];

            foreach (int index in _ids.Keys)
            {
                if (_names.TryGetValue(index, out string? name) &&
                    _arguments.TryGetValue(index, out StringBuilder? arguments))
                {
                    toolCalls.Add(ChatToolCall.CreateFunctionToolCall(
                        _ids[index],
                        name,
                        BinaryData.FromString(arguments.ToString())));
                }
            }

            return toolCalls;
        }
    }
}