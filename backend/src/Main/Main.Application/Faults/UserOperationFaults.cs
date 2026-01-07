using SharedKernel;

namespace Main.Application.Faults;

internal static class UserOperationFaults
{
    internal static readonly Fault NotFound = Fault.NotFound
    (
        title: "User.NotFound",
        detail: "The specified user was not found."
    );
}