using Main.Application.Abstractions.Stream;
using Main.Infrastructure.AI.Plugins;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Main.Infrastructure.AI.Filters;

internal sealed class ToolCallStreamFilter(
    PluginStreamContext pluginStreamContext,
    IStreamPublisher streamPublisher,
    ILogger<ToolCallStreamFilter> logger) : IAutoFunctionInvocationFilter
{
    private static readonly Dictionary<string, string> ToolDisplayNames = new()
    {
        ["__ws"] = "web_search",
        ["__sm"] = "save_memory"
    };

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        string functionName = context.Function.Name;

        if (pluginStreamContext.StreamId is not null &&
            ToolDisplayNames.TryGetValue(functionName, out var displayName))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "Tool call intercepted: {FunctionName} → streaming as {DisplayName} to stream {StreamId}",
                    functionName, displayName, pluginStreamContext.StreamId);

            await streamPublisher.PublishToolCallAsync
            (
                streamId: pluginStreamContext.StreamId,
                toolName: displayName,
                cancellationToken: context.CancellationToken
            );
        }

        await next(context);
    }
}