using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SharedKernel.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        if (Guid.TryParse(userId, out Guid parsedUserId))
            return parsedUserId;
        
        throw new InvalidOperationException("User id is unavailable");
    }
    
    public static Guid GetSessionId(this ClaimsPrincipal principal)
    {
        string? sessionId = principal.FindFirstValue(JwtRegisteredClaimNames.Sid);
        
        if (Guid.TryParse(sessionId, out Guid parsedSessionId))
            return parsedSessionId;
        
        throw new InvalidOperationException("Session id is unavailable");
    }
    
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        string? email = principal.FindFirstValue(JwtRegisteredClaimNames.Email);
        
        return email ?? throw new InvalidOperationException("Email is unavailable");
    }
}