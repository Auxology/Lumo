namespace Auth.Domain.Constants;

public static class SessionConstants
{
    public const int SessionExpirationDays = 30;
    
    public const int RefreshTokenKeyLength = 12;
    
    public const int RefreshTokenLength = 64;
}