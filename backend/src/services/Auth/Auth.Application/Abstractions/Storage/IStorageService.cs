namespace Auth.Application.Abstractions.Storage;

public interface IStorageService
{
    string GenerateFileKey(Guid userId);

    Task<PresignedUploadUrl> GeneratePresignedUploadUrlAsync
    (
        string fileKey,
        string contentType,
        long contentLength,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default
    );
}

public sealed record PresignedUploadUrl(Uri Url);