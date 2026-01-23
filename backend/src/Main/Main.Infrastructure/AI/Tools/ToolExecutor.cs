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
            "save_memory" => await ExecuteSaveMemoryAsync(chatToolCall, userId, cancellationToken),
            _ => $"Unknown function: {chatToolCall.FunctionName}"
        };
    }

    private async Task<string> ExecuteSaveMemoryAsync(ChatToolCall chatToolCall, Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            SaveMemoryArguments? arguments = JsonSerializer.Deserialize<SaveMemoryArguments>(
                chatToolCall.FunctionArguments.ToString());

            if (arguments is null)
                return "Failed to deserialize arguments.";

            if (!Enum.TryParse<MemoryCategory>(arguments.Category, ignoreCase: true, out MemoryCategory memoryCategory))
                return $"Invalid category: {arguments.Category}. Must be 'preference', 'fact', or 'instruction'.";

            string memoryId = await memoryStore.SaveAsync
            (
                userId: userId,
                content: arguments.Content,
                memoryCategory: memoryCategory,
                cancellationToken: cancellationToken
            );

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