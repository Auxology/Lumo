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
    
    public static Error AlreadyRevoked => Error.Conflict
    (
        title: "Session.AlreadyRevoked",
        detail: "The session has already been revoked."
    );
    
    public static Error Expired => Error.Unauthorized
    (
        title: "Session.Expired",
        detail: "The session has expired."
    );
    
    public static Error Invalid => Error.Unauthorized
    (
        title: "Session.Invalid",
        detail: "The session is invalid."
    );
}