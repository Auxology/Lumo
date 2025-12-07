namespace Auth.Domain.Constants;

public static class UserConstants
{
    public const int MaxDisplayNameLength = 100;
    
    public const int MaxEmailLength = 254;

    public const int TokenExpirationMinutes = 5;

    public const int MaxActiveTokens = 5;
    
    public const string OtpVerificationMethod = "OTP";
    
    public const string MagicLinkVerificationMethod = "MagicLink";
}