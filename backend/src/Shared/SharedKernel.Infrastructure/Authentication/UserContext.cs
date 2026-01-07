using Microsoft.AspNetCore.Http;

using SharedKernel.Application.Authentication;

namespace SharedKernel.Infrastructure.Authentication;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                          throw new InvalidOperationException("User context is unavailable");

    public string SessionId => httpContextAccessor.HttpContext?.User.GetSessionId() ??
                            throw new InvalidOperationException("Session id is unavailable");
}