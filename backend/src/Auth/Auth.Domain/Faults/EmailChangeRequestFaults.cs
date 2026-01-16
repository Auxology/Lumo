using SharedKernel;

namespace Auth.Domain.Faults;

public static class EmailChangeRequestFaults
{
    public static readonly Fault UserIdRequiredForCreation = Fault.Validation
    (
        title: "EmailChangeRequest.UserIdRequiredForCreation",
        detail: "A user ID is required to create an email change request."
    );

    public static readonly Fault TokenKeyRequiredForCreation = Fault.Validation
    (
        title: "EmailChangeRequest.TokenKeyRequiredForCreation",
        detail: "A token key is required to create an email change request."
    );

    public static readonly Fault CurrentEmailRequiredForCreation = Fault.Validation
    (
        title: "EmailChangeRequest.CurrentEmailRequiredForCreation",
        detail: "The current email address is required."
    );

    public static readonly Fault NewEmailRequiredForCreation = Fault.Validation
    (
        title: "EmailChangeRequest.NewEmailRequiredForCreation",
        detail: "A new email address is required."
    );

    public static readonly Fault EmailAddressesMustBeDifferent = Fault.Conflict
    (
        title: "EmailChangeRequest.EmailAddressesMustBeDifferent",
        detail: "The new email address must be different from the current one."
    );

    public static readonly Fault OtpTokenHashRequiredForCreation = Fault.Validation
    (
        title: "EmailChangeRequest.OtpTokenHashRequiredForCreation",
        detail: "OTP token hash is required."
    );

    public static readonly Fault MagicLinkTokenHashRequiredForCreation = Fault.Validation
    (
        title: "EmailChangeRequest.MagicLinkTokenHashRequiredForCreation",
        detail: "Magic link token hash is required."
    );

    public static readonly Fault AlreadyCompleted = Fault.Conflict
    (
        title: "EmailChangeRequest.AlreadyCompleted",
        detail: "This email change request has already been completed."
    );

    public static readonly Fault AlreadyCancelled = Fault.Conflict
    (
        title: "EmailChangeRequest.AlreadyCancelled",
        detail: "This email change request has been cancelled."
    );

    public static readonly Fault Expired = Fault.Unauthorized
    (
        title: "EmailChangeRequest.Expired",
        detail: "This email change request has expired. Please start a new request."
    );
}