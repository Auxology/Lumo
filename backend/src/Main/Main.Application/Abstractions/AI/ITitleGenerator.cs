namespace Main.Application.Abstractions.AI;

public interface ITitleGenerator
{
    Task<string> GetTitleAsync(string message, CancellationToken cancellationToken);
}