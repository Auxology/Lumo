using Auth.Application.Abstractions.Authentication;

namespace Auth.Application.Tests.TestHelpers;

internal sealed class FakeJwtTokenProvider : IJwtTokenProvider
{
    private int _refreshTokenCounter;

    public string CreateAccessToken(TokenClaims tokenClaims)
    {
        return $"access_token_for_{tokenClaims.UserId}";
    }

    public string GenerateRefreshToken()
    {
        _refreshTokenCounter++;
        return $"refresh_token_{_refreshTokenCounter}";
    }

    public void Reset()
    {
        _refreshTokenCounter = 0;
    }
}
