using Auth.Domain.Constants;
using SharedKernel;

namespace Auth.Domain.Faults;

public static class RecoveryKeyChainFaults
{
    public static readonly Fault UserIdRequiredForCreation = Fault.Validation
    (
        title: "RecoveryKeyChain.UserIdRequiredForCreation",
        detail: "A user ID is required to create recovery keys."
    );
    
    public static readonly Fault InvalidRecoveryKeyCount = Fault.Validation
    (
        title: "RecoveryKeyChain.InvalidRecoveryKeyCount",
        detail: $"Exactly {RecoveryKeyConstants.MaxKeysPerChain} recovery keys must be provided."
    );
}
