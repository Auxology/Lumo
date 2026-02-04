using System.Text.Json;

using Main.Application.Abstractions.Memory;

using Microsoft.Extensions.Logging;

using OpenAI.Chat;

namespace Main.Infrastructure.AI.Tools;

internal sealed class ToolExecutor(IMemoryStore memoryStore, ILogger<ToolExecutor> logger)
{
    public async Task<string> ExecuteAsync(ChatToolCall chatToolCall, Guid userId, CancellationToken cancellationToken)
    {
        return chatToolCall.FunctionName switch
        {
            "__sm" => await ExecuteSaveMemoryAsync(chatToolCall, userId, cancellationToken),
            _ => $"Unknown function: {chatToolCall.FunctionName}"
        };
    }

    private async Task<string> ExecuteSaveMemoryAsync(ChatToolCall chatToolCall, Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("ExecuteSaveMemoryAsync called for user {UserId}, function: {Function}",
            userId, chatToolCall.FunctionName);

        try
        {
            SaveMemoryArguments? arguments = JsonSerializer.Deserialize<SaveMemoryArguments>(
                chatToolCall.FunctionArguments.ToString());

            if (arguments is null)
            {
                logger.LogWarning("Failed to deserialize save_memory arguments for user {UserId}", userId);
                return "Failed to deserialize arguments.";
            }

            logger.LogInformation("Parsed save_memory: ContentLength={ContentLength}, Category={Category}",
                arguments.Content.Length, arguments.Category);

            if (!Enum.TryParse<MemoryCategory>(arguments.Category, ignoreCase: true, out MemoryCategory memoryCategory))
            {
                logger.LogWarning("Invalid category {Category} for user {UserId}", arguments.Category, userId);
                return $"Invalid category: {arguments.Category}. Must be 'preference', 'fact', or 'instruction'.";
            }

            string memoryId = await memoryStore.SaveAsync
            (
                userId: userId,
                content: arguments.Content,
                memoryCategory: memoryCategory,
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Memory saved successfully: {MemoryId} for user {UserId}", memoryId, userId);
            return $"Memory saved with ID: {memoryId}";
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(exception, "Invalid memory content from AI");
            return $"Failed to save memory: {exception.Message}";
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Memory limit reached for user {UserId}", userId);
            return $"Failed to save memory: {exception.Message}";
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Failed to deserialize save_memory arguments");
            return "Failed to parse function arguments";
        }
    }
}