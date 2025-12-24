namespace Auth.Application.Abstractions.Storage;

public interface IStorageService
{
    Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default);
}
