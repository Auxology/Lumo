using Auth.Domain.Constants;
using SharedKernel.ResultPattern;

namespace Auth.Domain.Errors;

public static class RecoveryCodeErrors
{
    public static readonly Error UserIdRequired = Error.Validation
    (
        title: "RecoveryCode.UserIdRequired",
        detail: "User ID is required to create a recovery code."
    );

    public static readonly Error CodeRequired = Error.Validation
    (
        title: "RecoveryCode.CodeRequired",
        detail: "Recovery code value is required."
    );

    public static readonly Error InvalidCodeLength = Error.Validation
    (
        title: "RecoveryCode.InvalidCodeLength",
        detail: $"Recovery code must be exactly {RecoveryCodeConstants.CodeLength} characters."
    );

    public static readonly Error AlreadyUsed = Error.Validation
    (
        title: "RecoveryCode.AlreadyUsed",
        detail: "This recovery code has already been used."
    );

    public static readonly Error Revoked = Error.Validation
    (
        title: "RecoveryCode.Revoked",
        detail: "This recovery code has been revoked."
    );

    public static readonly Error InvalidCode = Error.Validation
    (
        title: "RecoveryCode.InvalidCode",
        detail: "The provided recovery code is invalid."
    );

    public static readonly Error NoCodesAvailable = Error.Validation
    (
        title: "RecoveryCode.NoCodesAvailable",
        detail: "No valid recovery codes are available."
    );
}