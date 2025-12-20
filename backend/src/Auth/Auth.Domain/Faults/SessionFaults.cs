using SharedKernel;

namespace Auth.Domain.Faults;

public static class SessionFaults
{
    public static readonly Fault UserIdRequiredForCreation = Fault.Validation
    (
        title: "Session.UserIdRequiredForCreation",
        detail: "A user ID is required to create a session."
    );

    public static readonly Fault RefreshTokenKeyRequiredForCreation = Fault.Validation
    (
        title: "Session.RefreshTokenKeyRequiredForCreation",
        detail: "A refresh token key is required to create a session."
    );

    public static readonly Fault RefreshTokenHashRequiredForCreation = Fault.Validation
    (
        title: "Session.RefreshTokenHashRequiredForCreation",
        detail: "A refresh token hash is required to create a session."
    );

    public static readonly Fault RefreshTokenKeyRequiredForRefresh = Fault.Validation
    (
        title: "Session.RefreshTokenKeyRequiredForRefresh",
        detail: "A refresh token key is required to refresh the session."
    );

    public static readonly Fault RefreshTokenHashRequiredForRefresh = Fault.Validation
    (
        title: "Session.RefreshTokenHashRequiredForRefresh",
        detail: "A refresh token hash is required to refresh the session."
    );
    
    public static readonly Fault SessionRevoked = Fault.Validation
    (
        title: "Session.SessionRevoked",
        detail: "The session has been revoked."
    );
    
    public static readonly Fault SessionExpired = Fault.Validation
    (
        title: "Session.SessionExpired",
        detail: "The session has expired."
    );
}

