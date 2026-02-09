namespace Main.Application.Abstractions.AI;

public interface INativeChatCompletionService
{
    Task StreamCompletionAsync
    (
        string chatId,
        string streamId,
        string modelId,
        string correlationId,
        IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken
    );

    Task StreamCompletionAdvancedAsync
    (
        Guid userId,
        string chatId,
        string streamId,
        string modelId,
        string correlationId,
        IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken
    );
}