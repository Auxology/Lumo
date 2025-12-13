using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Application.Abstractions.Authentication;
using Auth.Domain.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Infrastructure.Options;

namespace Auth.Infrastructure.Authentication;

internal sealed class JwtTokenProvider(IOptions<JwtOptions> jwtOptions) : IJwtTokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    private static readonly JsonWebTokenHandler TokenHandler = new();

    public string CreateAccessToken(TokenClaims tokenClaims)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, tokenClaims.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, tokenClaims.SessionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, tokenClaims.EmailAddress),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience
        };

        return TokenHandler.CreateToken(tokenDescriptor);
    }

    public string GenerateSecret()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(SessionConstants.RefreshTokenLength);
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
