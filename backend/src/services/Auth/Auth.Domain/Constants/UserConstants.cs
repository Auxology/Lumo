namespace Auth.Domain.Constants;

public static class UserConstants
{
    public const int MaxDisplayNameLength = 100;
    
    public const int MaxEmailLength = 254;

    public const int TokenExpirationMinutes = 5;

    public const int MaxActiveTokens = 5;
    
    public const string OtpVerificationMethod = "OTP";
    
    public const string MagicLinkVerificationMethod = "MagicLink";

    public const int AvatarPresignedUrlExpirationMinutes = 15;
    
    public const int MaxAvatarSizeInBytes = 5 * 1024 * 1024;
    
    public static readonly HashSet<string> AllowedContentTypes = 
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}