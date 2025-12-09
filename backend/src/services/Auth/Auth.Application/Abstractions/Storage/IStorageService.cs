namespace Auth.Application.Abstractions.Storage;

public interface IStorageService
{
    string GenerateFileKey(Guid userId);

    Task<PresignedUploadUrl> GeneratePresignedUploadUrlAsync
    (
        string fileKey,
        string contentType,
        long contentLength, 
        CancellationToken cancellationToken = default
    );
    
    Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default);
    
    bool ValidateKeyOwnership(string fileKey, Guid userId);
}

public sealed record PresignedUploadUrl(Uri Url);