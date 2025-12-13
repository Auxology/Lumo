using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Application.Authentication;
using SharedKernel.Infrastructure.Options;

namespace SharedKernel.Infrastructure.Authentication;

public sealed class JwtTokenValidator(IOptions<JwtOptions> options) : IJwtTokenValidator
{
    private readonly JwtOptions _options = options.Value;

    private static readonly JsonWebTokenHandler JsonWebTokenHandler = new();

    public async Task<bool> IsValid(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return false;

        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_options.Secret)),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };

        TokenValidationResult result = await JsonWebTokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);

        return result.IsValid;
    }
}
