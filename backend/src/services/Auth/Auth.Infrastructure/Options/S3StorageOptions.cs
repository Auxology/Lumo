using System.ComponentModel.DataAnnotations;

namespace Auth.Infrastructure.Options;

public sealed class S3StorageOptions
{
    public const string SectionName = "S3Storage";

    [Required(ErrorMessage = "S3 bucket name is required.")]
    [MinLength(3, ErrorMessage = "Bucket name must be at least 3 characters.")]
    [MaxLength(63, ErrorMessage = "Bucket name cannot exceed 63 characters.")]
    public string BucketName { get; init; } = string.Empty;

    [Required(ErrorMessage = "AWS region is required.")]
    public string Region { get; init; } = string.Empty;

    [Required(ErrorMessage = "Avatar key prefix is required.")]
    public string AvatarKeyPrefix { get; init; } = "avatars";
}