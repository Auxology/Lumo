namespace Main.Application.Abstractions.Stream;

public interface IChatLockService
{
    Task<bool> TryAcquireLockAsync(string chatId, CancellationToken cancellationToken);

    Task ReleaseLockAsync(string chatId, CancellationToken cancellationToken);

    Task<bool> IsGeneratingAsync(string chatId, CancellationToken cancellationToken);
}