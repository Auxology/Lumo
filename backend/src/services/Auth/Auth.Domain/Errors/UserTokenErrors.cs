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

    public static Error OtpTokenRequired => Error.Validation
    (
        title: "User.OtpTokenRequired",
        detail: "OTP token is required to request login."
    );

    public static Error MagicLinkTokenRequired => Error.Validation
    (
        title: "User.MagicLinkTokenRequired",
        detail: "Magic link token is required to request login."
    );
}