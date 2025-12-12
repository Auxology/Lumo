using SharedKernel.ResultPattern;

namespace Auth.Application.Errors;

public static class SessionOperationErrors
{
    public static Error InvalidRefreshToken => Error.Unauthorized
    (
        title: "Session.InvalidRefreshToken",
        detail: "The provided refresh token is invalid or expired."
    );

    public static Error SessionNotFound => Error.NotFound
    (
        title: "Session.NotFound",
        detail: "The specified session was not found."
    );
}
