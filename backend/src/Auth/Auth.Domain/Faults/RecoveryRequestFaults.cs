using SharedKernel;

namespace Auth.Domain.Faults;

public static class RecoveryRequestFaults
{
    public static readonly Fault UserIdRequiredForCreation = Fault.Validation
    (
        title: "RecoveryRequest.UserIdRequiredForCreation",
        detail: "A user ID is required to create a recovery request."
    );

    public static readonly Fault TokenKeyRequiredForCreation = Fault.Validation
    (
        title: "RecoveryRequest.TokenKeyRequiredForCreation",
        detail: "A token key is required to create a recovery request."
    );

    public static readonly Fault NewEmailRequiredForCreation = Fault.Validation
    (
        title: "RecoveryRequest.NewEmailRequiredForCreation",
        detail: "A new email address is required to create a recovery request."
    );

    public static readonly Fault OtpTokenHashRequiredForCreation = Fault.Validation
    (
        title: "RecoveryRequest.OtpTokenHashRequiredForCreation",
        detail: "An OTP token hash is required to create a recovery request."
    );

    public static readonly Fault MagicLinkTokenHashRequiredForCreation = Fault.Validation
    (
        title: "RecoveryRequest.MagicLinkTokenHashRequiredForCreation",
        detail: "A magic link token hash is required to create a recovery request."
    );
    
    public static readonly Fault Expired = Fault.Validation
    (
        title: "RecoveryRequest.Expired",
        detail: "The recovery request has expired."
    );

    public static readonly Fault AlreadyCompleted = Fault.Validation
    (
        title: "RecoveryRequest.AlreadyCompleted",
        detail: "The recovery request has already been completed."
    );

    public static readonly Fault NewEmailNotVerified = Fault.Validation
    (
        title: "RecoveryRequest.NewEmailNotVerified",
        detail: "The new email must be verified before completing the recovery."
    );

    public static readonly Fault NewEmailAlreadyVerified = Fault.Validation
    (
        title: "RecoveryRequest.NewEmailAlreadyVerified",
        detail: "The new email has already been verified for this recovery request."
    );
    
}