using SharedKernel;

namespace Main.Api.Faults;

internal static class RateLimitingFaults
{
    internal static Fault AiGenerationLimitExceeded => Fault.TooManyRequests
    (
        title: "AiGeneration.RateLimitExceeded",
        detail: "You have exceeded the AI generation rate limit. Please wait before sending more messages."
    );
}