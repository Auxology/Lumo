namespace Auth.Application.Abstractions.Storage;

public static class AvatarConstants
{
    public const string AvatarFolder = "avatars";

    public const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public const int PresignedUrlExpirationMinutes = 15;

    public static readonly IReadOnlyList<string> AllowedContentTypes = new[]
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };
}
