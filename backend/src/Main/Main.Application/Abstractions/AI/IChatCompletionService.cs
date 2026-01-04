namespace Main.Application.Abstractions.AI;

public interface IChatCompletionService
{
    Task<string> GetTitleAsync(string message, CancellationToken cancellationToken);

    Task StreamCompletionAsync(Guid chatId, IReadOnlyList<ChatCompletionMessage> messages,
        CancellationToken cancellationToken);
}