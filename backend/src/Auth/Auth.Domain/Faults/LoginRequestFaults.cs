using SharedKernel;

namespace Auth.Domain.Faults;

public static class LoginRequestFaults
{
    public static readonly Fault UserIdRequiredForCreation = Fault.Validation
    (
        title: "LoginRequest.UserIdRequiredForCreation",
        detail: "A user ID is required to create a login request."
    );

    public static readonly Fault TokenKeyRequiredForCreation = Fault.Validation
    (
        title: "LoginRequest.TokenKeyRequiredForCreation",
        detail: "A token key is required to create a login request."
    );

    public static readonly Fault OtpTokenHashRequiredForCreation = Fault.Validation
    (
        title: "LoginRequest.OtpTokenHashRequiredForCreation",
        detail: "An OTP token hash is required to create a login request."
    );

    public static readonly Fault MagicLinkTokenHashRequiredForCreation = Fault.Validation
    (
        title: "LoginRequest.MagicLinkTokenHashRequiredForCreation",
        detail: "A magic link token hash is required to create a login request."
    );
    
    public static readonly Fault InvalidOrExpired = Fault.Unauthorized
    (
        title: "LoginRequest.InvalidOrExpired",
        detail: "The provided token is invalid or has expired."
    );
}

