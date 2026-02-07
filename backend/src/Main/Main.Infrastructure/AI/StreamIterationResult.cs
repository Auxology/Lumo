using OpenAI.Chat;

namespace Main.Infrastructure.AI;

internal sealed record StreamIterationResult
(
    IReadOnlyList<ChatToolCall> ToolCalls,
    string Content,
    bool HasProviderError
);