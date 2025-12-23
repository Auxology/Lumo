using System.Security.Claims;
using System.Text;
using Auth.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Infrastructure.Options;

namespace Auth.Infrastructure.Authentication;

internal sealed class TokenProvider(JsonWebTokenHandler jsonWebTokenHandler, IOptions<JwtOptions> jwtOptions) : ITokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    private readonly SigningCredentials _signingCredentials =
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey)),
            SecurityAlgorithms.HmacSha256);
    
    public string CreateToken(Guid userId, Guid sessionId)
    {
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity
            ([
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, sessionId.ToString())
            ]),
            Expires = DateTime.UtcNow.Add(_jwtOptions.AccessTokenExpiration),
            SigningCredentials = _signingCredentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience
        };

        string token = jsonWebTokenHandler.CreateToken(tokenDescriptor);

        return token;
    }
}