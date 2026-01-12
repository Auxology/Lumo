namespace Main.Application.Abstractions.AI;

public interface IChatCompletionService
{
    Task<string> GetTitleAsync(string message, CancellationToken cancellationToken);

    Task StreamCompletionAsync(string chatId, string streamId, IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken);
}