using System.ComponentModel;

using Main.Application.Abstractions.Memory;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Main.Infrastructure.AI.Plugins;

internal sealed class MemoryPlugin
(
    PluginUserContext userContext,
    IMemoryStore memoryStore,
    ILogger<MemoryPlugin> logger
)
{
    [KernelFunction("__sm")]
    [Description
        (
            "Save important information about the user to memory for future conversations. " +
            "Use when the user shares preferences, personal facts, or instructions they want remembered. " +
            "Examples: 'I prefer dark mode', 'My name is John', 'Always respond in Spanish'."
        )
    ]
    public async Task<string> SaveMemoryAsync
    (
        [Description("The specific information to remember about the user. Be concise but complete.")]
        string content,
        [Description(
            "The type of memory: 'preference' for user preferences, 'fact' for personal information, 'instruction' for behavioral guidelines.")]
        string category,
        CancellationToken cancellationToken
    )
    {
        Guid userId = userContext.UserId;

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "SaveMemoryAsync called for user {UserId}, content length: {ContentLength}, category: {Category}",
                userId, content.Length, category);

        try
        {
            if (!Enum.TryParse<MemoryCategory>(category, ignoreCase: true, out MemoryCategory memoryCategory))
            {
                logger.LogWarning("Invalid category {Category} for user {UserId}", category, userId);
                return $"Invalid category: {category}. Must be 'preference', 'fact', or 'instruction'.";
            }

            string memoryId = await memoryStore.SaveAsync
            (
                userId: userId,
                content: content,
                memoryCategory: memoryCategory,
                cancellationToken: cancellationToken
            );

            if (logger.IsEnabled(LogLevel.Information))
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
    }
}