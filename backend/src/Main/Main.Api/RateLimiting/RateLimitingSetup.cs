using System.Globalization;
using System.Threading.RateLimiting;

using Main.Api.Faults;
using Main.Api.Options;

using SharedKernel;
using SharedKernel.Api.Infrastructure;
using SharedKernel.Infrastructure.Authentication;
using SharedKernel.Infrastructure.Options;

namespace Main.Api.RateLimiting;

internal static class RateLimitingSetup
{
    internal static IServiceCollection AddRateLimitingSetup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        RateLimitingOptions rateLimitingOptions = new();
        configuration.GetSection(RateLimitingOptions.SectionName).Bind(rateLimitingOptions);

        ValkeyOptions valkeyOptions = new();
        configuration.GetSection(ValkeyOptions.SectionName).Bind(valkeyOptions);

        if (!rateLimitingOptions.Enabled)
            return services;

        return services.AddInMemoryRateLimiting(rateLimitingOptions);
    }

    private static IServiceCollection AddInMemoryRateLimiting(this IServiceCollection services,
        RateLimitingOptions options)
    {
        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                Outcome outcome = Outcome.Failure(RateLimitingFaults.AiGenerationLimitExceeded);
                await CustomResults.Problem(outcome, context.HttpContext)
                    .ExecuteAsync(context.HttpContext);
            };

            limiterOptions.AddPolicy("ai-generation", context =>
            {
                Guid userId = context.User.GetUserId();

                return RateLimitPartition.GetTokenBucketLimiter
                (
                    partitionKey: userId,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = options.AiGenerationTokenLimit,
                        TokensPerPeriod = options.AiGenerationTokensPerPeriod,
                        ReplenishmentPeriod = options.AiGenerationReplenishmentPeriod,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}