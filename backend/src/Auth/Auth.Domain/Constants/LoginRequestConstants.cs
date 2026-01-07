namespace Auth.Domain.Constants;

public static class LoginRequestConstants
{
    public const int ExpirationMinutes = 10;

    public const int TokenKeyLength = 32;

    public const int OtpTokenLength = 8;

    public const int MagicLinkTokenLength = 64;
}