using Auth.Application.Abstractions.Storage;

namespace Auth.Application.Tests.TestHelpers;

internal sealed class FakeStorageService : IStorageService
{
    private readonly HashSet<string> _uploadedFiles = new();

    public string GenerateFileKey(Guid userId)
    {
        return $"users/{userId}/files/{Guid.NewGuid()}";
    }

    public Task<PresignedUploadUrl> GeneratePresignedUploadUrlAsync(
        string fileKey,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri($"https://fake-storage.com/upload/{fileKey}?contentType={contentType}&size={contentLength}");
        return Task.FromResult(new PresignedUploadUrl(url));
    }

    public Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_uploadedFiles.Contains(fileKey));
    }

    public bool ValidateKeyOwnership(string fileKey, Guid userId)
    {
        return fileKey.StartsWith($"users/{userId}/", StringComparison.Ordinal);
    }

    public void SimulateFileUpload(string fileKey)
    {
        _uploadedFiles.Add(fileKey);
    }

    public void Clear() => _uploadedFiles.Clear();
}
