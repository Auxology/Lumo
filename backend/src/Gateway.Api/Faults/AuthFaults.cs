using SharedKernel;

namespace Gateway.Api.Faults;

internal static class AuthFaults
{
    internal static readonly Fault FailedToDeserialize = Fault.Problem
    (
        title: "Auth.FailedToDeserialize",
        detail: "Failed to deserialize response from Auth Service."
    );
}
