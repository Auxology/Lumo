using SharedKernel.ResultPattern;

namespace Auth.Domain.Errors;

public static class SessionErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        title: "Session.UserIdRequired",
        detail: "UserId is required to create a session."
    );

    public static Error HashedSecretRequired => Error.Validation
    (
        title: "Session.HashedSecretRequired",
        detail: "Hashed secret is required to create or refresh a session."
    );

    public static Error SecretRequired => Error.Validation
    (
        title: "Session.SecretRequired",
        detail: "Secret is required to verify a session."
    );

    public static Error InvalidSecret => Error.Unauthorized
    (
        title: "Session.InvalidSecret",
        detail: "The provided secret is invalid."
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
