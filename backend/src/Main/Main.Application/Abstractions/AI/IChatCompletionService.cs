namespace Main.Application.Abstractions.AI;

public interface IChatCompletionService
{
    Task<string> GetTitleAsync(string message, CancellationToken cancellationToken);
}
