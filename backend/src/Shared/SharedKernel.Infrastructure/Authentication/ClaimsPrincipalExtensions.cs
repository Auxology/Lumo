using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace SharedKernel.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        string? userId = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userId, out Guid parsedUserId) 
            ? parsedUserId 
            : throw new InvalidOperationException("User id is unavailable");
    }
    
    public static Guid GetSessionId(this ClaimsPrincipal claimsPrincipal)
    {
        string? sessionId = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sid);

        return Guid.TryParse(sessionId, out Guid parsedSessionId) 
            ? parsedSessionId 
            : throw new InvalidOperationException("Session id is unavailable");
    }
}