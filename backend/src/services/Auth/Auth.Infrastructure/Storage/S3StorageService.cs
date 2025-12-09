using System.Globalization;
using Amazon.S3;
using Amazon.S3.Model;
using Auth.Application.Abstractions.Storage;
using Auth.Domain.Constants;
using Auth.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure.Time;
using SharedKernel.Time;

namespace Auth.Infrastructure.Storage;

internal sealed class S3StorageService(
    AmazonS3Client s3Client,
    IOptions<S3StorageOptions> storageOptions,
    IDateTimeProvider dateTimeProvider,
    ILogger<S3StorageService> logger) : IStorageService
{
    private readonly S3StorageOptions _storageOptions = storageOptions.Value;

    public string GenerateFileKey(Guid userId)
    {
        string uniqueId = Guid.CreateVersion7().ToString("N")[..8];
        string fileKey = $"{_storageOptions.AvatarKeyPrefix}/{userId}/{uniqueId}/avatar";

        logger.LogInformation
        (
            "Generated file key for user {UserId}: {FileKey}",
            userId,
            fileKey
        );

        return fileKey;
    }

    public async Task<PresignedUploadUrl> GeneratePresignedUploadUrlAsync(string fileKey, string contentType, long contentLength,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation
        (
            "Generating presigned upload URL for file key {FileKey} with content type {ContentType} and length {ContentLength} bytes",
            fileKey,
            contentType,
            contentLength
        );

        if (contentLength <= 0)
        {
            logger.LogWarning
            (
                "Invalid content length {ContentLength} for file key {FileKey}",
                contentLength,
                fileKey
            );
            
            throw new ArgumentException("Content length must be greater than zero.", nameof(contentLength));
        }

        if (contentLength > UserConstants.MaxAvatarSizeInBytes)
        {
            logger.LogWarning
            (
                "Content length {ContentLength} exceeds maximum allowed size {MaxSize} bytes for file key {FileKey}",
                contentLength,
                UserConstants.MaxAvatarSizeInBytes,
                fileKey
            );
            
            throw new ArgumentException
            (
                $"Content length exceeds the maximum allowed size of {UserConstants.MaxAvatarSizeInBytes} bytes.",
                nameof(contentLength)
            );
        }

        if (!UserConstants.AllowedContentTypes.Contains(contentType))
        {
            logger.LogWarning
            (
                "Invalid content type {ContentType} for file key {FileKey}. Allowed types: {AllowedTypes}",
                contentType,
                fileKey,
                string.Join(", ", UserConstants.AllowedContentTypes)
            );
            throw new ArgumentException("Content type is not allowed.", nameof(contentType));
        }

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        DateTime expiresAt =
            dateTimeProvider.ToDateTime(utcNow.AddMinutes(UserConstants.AvatarPresignedUrlExpirationMinutes));

        GetPreSignedUrlRequest request = new()
        {
            BucketName = _storageOptions.BucketName,
            Key = fileKey,
            Verb = HttpVerb.PUT,
            Expires = expiresAt,
            ContentType = contentType
        };

        request.Metadata.Add("x-amz-meta-content-length", contentLength.ToString(CultureInfo.InvariantCulture));

        logger.LogInformation
        (
            "Creating S3 presigned URL request for bucket {BucketName}, key {FileKey}, expires at {ExpiresAt}",
            _storageOptions.BucketName,
            fileKey,
            expiresAt
        );

        try
        {
            string presignedUrl = await s3Client.GetPreSignedURLAsync(request);

            logger.LogInformation
            (
                "Successfully generated presigned upload URL for file key {FileKey}",
                fileKey
            );

            return new PresignedUploadUrl(new Uri(presignedUrl));
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError
            (
                exception,
                "Failed to generate presigned URL for file key {FileKey} in bucket {BucketName}. Status: {StatusCode}",
                fileKey,
                _storageOptions.BucketName,
                exception.StatusCode
            );

            throw new InvalidOperationException
            (
                $"Failed to generate presigned upload URL for file key '{fileKey}' in bucket '{_storageOptions.BucketName}'. Status: {exception.StatusCode}",
                exception
            );
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        logger.LogInformation
        (
            "Checking if file exists in S3: {FileKey}",
            fileKey
        );

        try
        {
            GetObjectMetadataRequest request = new()
            {
                BucketName = _storageOptions.BucketName,
                Key = fileKey
            };

            GetObjectMetadataResponse response = await s3Client.GetObjectMetadataAsync(request, cancellationToken);

            logger.LogInformation
            (
                "File exists in S3: {FileKey}, Size: {ContentLength} bytes, LastModified: {LastModified}",
                fileKey,
                response.ContentLength,
                response.LastModified
            );

            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation
            (
                exception,
                "File not found in S3: {FileKey}",
                fileKey
            );

            return false;
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError
            (
                exception,
                "Error checking file existence in S3 for key {FileKey} in bucket {BucketName}. Status: {StatusCode}",
                fileKey,
                _storageOptions.BucketName,
                exception.StatusCode
            );

            throw new InvalidOperationException
            (
                $"Failed to check file existence for key '{fileKey}' in bucket '{_storageOptions.BucketName}'. Status: {exception.StatusCode}",
                exception
            );
        }
    }

    public bool ValidateKeyOwnership(string fileKey, Guid userId)
    {
        string expectedPrefix = $"{_storageOptions.AvatarKeyPrefix}/{userId}/";
        
        return fileKey.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
    }
}