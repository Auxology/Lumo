namespace Main.Application.Abstractions.AI;

public interface IChatCompletionService
{
    Task<string> GetTitleAsync(string message, CancellationToken cancellationToken);

    Task StreamCompletionAsync(string chatId, string streamId, string modelId,
        IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken);
}