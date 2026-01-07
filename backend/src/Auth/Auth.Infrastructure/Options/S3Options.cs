using System.ComponentModel.DataAnnotations;

namespace Auth.Infrastructure.Options;

internal sealed class S3Options
{
    public const string SectionName = "S3";

    [Required, MinLength(1)]
    public string BucketName { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Region { get; init; } = string.Empty;
}