using SharedKernel;

namespace Gateway.Api.Faults;

internal static class RateLimitingFaults
{
    internal static Fault RateLimited => Fault.TooManyRequests
    (
        title: "General.RateLimited",
        detail: "Rate limit exceeded. Please try again later."
    );
}