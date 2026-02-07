namespace Main.Application.Abstractions.Stream;

public interface IChatLockService
{
    Task<bool> TryAcquireLockAsync(string chatId, string ownerId, CancellationToken cancellationToken);

    Task ReleaseLockAsync(string chatId, string ownerId, CancellationToken cancellationToken);

    Task<bool> IsGeneratingAsync(string chatId, CancellationToken cancellationToken);
}