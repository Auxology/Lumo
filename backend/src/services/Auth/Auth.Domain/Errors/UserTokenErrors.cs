using SharedKernel.ResultPattern;

namespace Auth.Domain.Errors;

public static class UserTokenErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        title: "UserToken.UserIdRequired",
        detail: "UserId is required to create a user token."
    );
    
    public static Error OtpTokenHashRequired => Error.Validation
    (
        title: "UserToken.OtpTokenHashRequired",
        detail: "OTP token hash is required to create a user token."
    );
    
    public static Error MagicLinkTokenHashRequired => Error.Validation
    (
        title: "UserToken.MagicLinkTokenHashRequired",
        detail: "Magic link token hash is required to create a user token."
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