namespace Auth.Domain.Constants;

public static class RecoveryRequestConstants
{
    public const int ExpirationMinutes = 15;
    
    public const int TokenKeyLength = 32;
    
    public const int OtpTokenLength = 8;
    
    public const int MagicLinkTokenLength = 64;
}