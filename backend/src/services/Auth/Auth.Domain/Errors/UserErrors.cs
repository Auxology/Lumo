using Auth.Domain.Constants;
using SharedKernel.ResultPattern;

namespace Auth.Domain.Errors;

public static class UserErrors
{
    public static Error DisplayNameRequired => Error.Validation
    (
        title: "User.DisplayNameRequired",
        detail: "Display name is required."
    );

    public static Error DisplayNameTooLong => Error.Validation
    (
        title: "User.DisplayNameTooLong",
        detail: $"Display name exceeds the maximum length of {UserConstants.MaxDisplayNameLength} characters."
    );
    
    public static Error EmailAddressRequired => Error.Validation
    (
        title: "User.EmailAddressRequired",
        detail: "Email address is required."
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
    
    public static Error TokenNotFoundOrExpired => Error.NotFound
    (
        title: "User.TokenNotFoundOrExpired",
        detail: "The token was not found or has expired."
    );
}