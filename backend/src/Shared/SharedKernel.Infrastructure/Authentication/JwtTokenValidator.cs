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
    private static readonly JsonWebTokenHandler JsonWebTokenHandler = new();


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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        TokenValidationResult result =
            await JsonWebTokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);
        
        return result.IsValid;
    }
}