using SharedKernel.ResultPattern;

namespace Auth.Domain.Errors;

public static class UserTokenErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        title: "UserToken.UserIdRequired",
        detail: "UserId is required to create a user token."
    );
    
    public static Error OtpTokenRequired => Error.Validation
    (
        title: "UserToken.OtpTokenRequired",
        detail: "OTP token is required to create a user token."
    );
    
    public static Error MagicLinkTokenRequired => Error.Validation
    (
        title: "UserToken.MagicLinkTokenRequired",
        detail: "Magic link token is required to create a user token."
    );
    
    public static Error AlreadyUsed => Error.Conflict
    (
        title: "UserToken.AlreadyUsed",
        detail: "The token has already been used."
    );

    public static Error Expired => Error.Validation
    (
        title: "UserToken.Expired",
        detail: "The token has expired and is no longer valid."
    );
    
    public static Error InvalidToken => Error.Validation
    (
        title: "UserToken.InvalidToken",
        detail: "The provided token is invalid."
    );
}