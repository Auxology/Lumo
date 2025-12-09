using System.Security.Cryptography;
using Auth.Application.Abstractions.Authentication;
using Auth.Domain.Constants;

namespace Auth.Infrastructure.Authentication;

public sealed class AuthTokenGenerator : IAuthTokenGenerator
{
    public string GenerateOtpToken()
    {
        return string.Create(UserTokenConstants.OtpTokenLength, 0, static (span, _) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
            }
        });
    }

    public string GenerateMagicLinkToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(UserTokenConstants.MagicLinkTokenLength);
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}