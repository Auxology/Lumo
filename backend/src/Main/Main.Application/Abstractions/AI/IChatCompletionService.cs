namespace Main.Application.Abstractions.AI;

public interface IChatCompletionService
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