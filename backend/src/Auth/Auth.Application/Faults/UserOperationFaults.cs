using SharedKernel;

namespace Auth.Application.Faults;

internal static class UserOperationFaults
{
    public static readonly Fault EmailAlreadyInUse = Fault.Conflict
    (
        title: "User.EmailAlreadyInUse",
        detail: "The provided email address is already associated with an existing account."
    );
}