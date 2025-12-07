using SharedKernel.ResultPattern;

namespace Auth.Domain.Errors;

public static class SessionErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        title: "Session.UserIdRequired",
        detail: "UserId is required to create a session."
    );
    
    public static Error HashedRefreshTokenRequired => Error.Validation
    (
        title: "Session.HashedRefreshTokenRequired",
        detail: "Hashed refresh token is required to create a session."
    );
}