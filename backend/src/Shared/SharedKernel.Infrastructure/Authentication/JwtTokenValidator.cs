using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using SharedKernel.Application.Authentication;
using SharedKernel.Infrastructure.Options;

namespace SharedKernel.Infrastructure.Authentication;

public sealed class JwtTokenValidator(IOptions<JwtOptions> jwtOptions) : IJwtTokenValidator
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler = new();

    private readonly SecurityKey _securityKey =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));

    public async Task<bool> IsValidAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return false;

        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = _securityKey,
            ClockSkew = TimeSpan.Zero
        };

        TokenValidationResult result =
            await _jsonWebTokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);

        return result.IsValid;
    }
}