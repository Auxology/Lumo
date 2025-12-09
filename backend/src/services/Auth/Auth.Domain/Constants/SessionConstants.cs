namespace Auth.Domain.Constants;

public static class SessionConstants
{
    public const int ExpirationDays = 30;
    
    public const int AbsoluteExpirationDays = 180;
    
    public const string RevokedByUser = "Revoked by user";
    
    public const int RefreshTokenLength = 64;
}