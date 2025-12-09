namespace Auth.Application.Abstractions.Authentication;

public sealed record TokenClaims(Guid UserId, Guid SessionId, string EmailAddress);

public interface IJwtTokenProvider
{
    string CreateAccessToken(TokenClaims tokenClaims);
    
    string GenerateRefreshToken();
}