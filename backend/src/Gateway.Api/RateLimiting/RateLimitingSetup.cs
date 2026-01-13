using System.Globalization;

using Gateway.Api.Faults;
using Gateway.Api.Options;

using Microsoft.AspNetCore.RateLimiting;

using RedisRateLimiting.AspNetCore;

using SharedKernel;
using SharedKernel.Api.Infrastructure;
using SharedKernel.Infrastructure.Options;

using StackExchange.Redis;

namespace Gateway.Api.RateLimiting;

internal static class RateLimitingSetup
{
    internal static IServiceCollection AddRateLimitingSetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        RateLimitingOptions rateLimitingOptions = new();
        configuration.GetSection(RateLimitingOptions.SectionName).Bind(rateLimitingOptions);

        if (!rateLimitingOptions.Enabled)
            return services;

        ValkeyOptions valkeyOptions = new();
        configuration.GetSection(ValkeyOptions.SectionName).Bind(valkeyOptions);

        if (!valkeyOptions.Enabled)
            return services.AddInMemoryRateLimiting(rateLimitingOptions);

        return services.AddDistributedRateLimiting(valkeyOptions, rateLimitingOptions);
    }

    private static IServiceCollection AddInMemoryRateLimiting(this IServiceCollection services,
        RateLimitingOptions options)
    {
        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(System.Threading.RateLimiting.MetadataName.RetryAfter,
                        out TimeSpan retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                
                Outcome outcome = Outcome.Failure(RateLimitingFaults.RateLimited);
                await CustomResults.Problem(outcome, context.HttpContext)
                    .ExecuteAsync(context.HttpContext);
            };

            limiterOptions.AddSlidingWindowLimiter("global", windowOptions =>
            {
                windowOptions.PermitLimit = options.GlobalPermitLimit;
                windowOptions.Window = TimeSpan.FromSeconds(options.GlobalWindowSeconds);
                windowOptions.SegmentsPerWindow = 4;
            });

            limiterOptions.AddSlidingWindowLimiter("auth", windowOptions =>
            {
                windowOptions.PermitLimit = options.AuthPermitLimit;
                windowOptions.Window = TimeSpan.FromSeconds(options.AuthWindowSeconds);
                windowOptions.SegmentsPerWindow = 4;
            });

            limiterOptions.AddSlidingWindowLimiter("chat", windowOptions =>
            {
                windowOptions.PermitLimit = options.ChatPermitLimit;
                windowOptions.Window = TimeSpan.FromSeconds(options.ChatWindowSeconds);
                windowOptions.SegmentsPerWindow = 4;
            });

            limiterOptions.AddTokenBucketLimiter("message", bucketOptions =>
            {
                bucketOptions.TokenLimit = options.MessageTokenLimit;
                bucketOptions.TokensPerPeriod = options.MessageTokensPerPeriod;
                bucketOptions.ReplenishmentPeriod = options.MessageReplenishmentPeriod;
            });
        });

        return services;
    }

    private static IServiceCollection AddDistributedRateLimiting
    (
        this IServiceCollection services,
        ValkeyOptions valkeyOptions,
        RateLimitingOptions rateLimitingOptions
    )
    {
        IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(valkeyOptions.ConnectionString);

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(System.Threading.RateLimiting.MetadataName.RetryAfter,
                        out TimeSpan retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                
                Outcome outcome = Outcome.Failure(RateLimitingFaults.RateLimited);
                await CustomResults.Problem(outcome, context.HttpContext)
                    .ExecuteAsync(context.HttpContext);
            };

            limiterOptions.AddRedisSlidingWindowLimiter("global", windowOptions =>
            {
                windowOptions.ConnectionMultiplexerFactory = () => connectionMultiplexer;
                windowOptions.PermitLimit = rateLimitingOptions.GlobalPermitLimit;
                windowOptions.Window = TimeSpan.FromSeconds(rateLimitingOptions.GlobalWindowSeconds);
            });

            limiterOptions.AddRedisSlidingWindowLimiter("auth", windowOptions =>
            {
                windowOptions.ConnectionMultiplexerFactory = () => connectionMultiplexer;
                windowOptions.PermitLimit = rateLimitingOptions.AuthPermitLimit;
                windowOptions.Window = TimeSpan.FromSeconds(rateLimitingOptions.AuthWindowSeconds);
            });

            limiterOptions.AddRedisSlidingWindowLimiter("chat", windowOptions =>
            {
                windowOptions.ConnectionMultiplexerFactory = () => connectionMultiplexer;
                windowOptions.PermitLimit = rateLimitingOptions.ChatPermitLimit;
                windowOptions.Window = TimeSpan.FromSeconds(rateLimitingOptions.ChatWindowSeconds);
            });

            limiterOptions.AddRedisTokenBucketLimiter("message", bucketOptions =>
            {
                bucketOptions.ConnectionMultiplexerFactory = () => connectionMultiplexer;
                bucketOptions.TokenLimit = rateLimitingOptions.MessageTokenLimit;
                bucketOptions.TokensPerPeriod = rateLimitingOptions.MessageTokensPerPeriod;
                bucketOptions.ReplenishmentPeriod = rateLimitingOptions.MessageReplenishmentPeriod;
            });
        });

        return services;
    }
}