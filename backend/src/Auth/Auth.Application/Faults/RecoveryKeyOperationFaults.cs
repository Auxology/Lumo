using SharedKernel;

namespace Auth.Application.Faults;

internal static class RecoveryKeyOperationFaults
{
    internal static readonly Fault Invalid = Fault.Unauthorized
    (
        title: "RecoveryKey.Invalid",
        detail: "The recovery key is invalid."
    );
}