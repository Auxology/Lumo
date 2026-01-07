using SharedKernel;

namespace Auth.Domain.Faults;

public static class RecoveryKeyFaults
{
    public static readonly Fault RecoveryKeyChainIdRequiredForCreation = Fault.Validation
    (
        title: "RecoveryKey.RecoveryKeyChainIdRequiredForCreation",
        detail: "A recovery key chain ID is required to create a recovery key."
    );

    public static readonly Fault IdentifierRequiredForCreation = Fault.Validation
    (
        title: "RecoveryKey.IdentifierRequiredForCreation",
        detail: "An identifier is required to create a recovery key."
    );

    public static readonly Fault VerifierHashRequiredForCreation = Fault.Validation
    (
        title: "RecoveryKey.VerifierHashRequiredForCreation",
        detail: "A verifier hash is required to create a recovery key."
    );
}