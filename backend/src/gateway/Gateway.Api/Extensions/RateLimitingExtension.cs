using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RedisRateLimiting.AspNetCore;
using SharedKernel.Infrastructure.Options;
using StackExchange.Redis;

namespace Gateway.Api.Extensions;

internal static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ValkeyOptions>()
            .Bind(configuration.GetSection(ValkeyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ValkeyOptions>>().Value;
            var config = ConfigurationOptions.Parse(options.ConnectionString);

            if (!string.IsNullOrEmpty(options.Password))
                config.Password = options.Password;

            return ConnectionMultiplexer.Connect(config);
        });

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = 429;

            rateLimiterOptions.AddRedisFixedWindowLimiter("fixed", opt =>
            {
                opt.ConnectionMultiplexerFactory = () =>
                    services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromSeconds(10);
            });

            rateLimiterOptions.AddRedisSlidingWindowLimiter("sliding", opt =>
            {
                opt.ConnectionMultiplexerFactory = () =>
                    services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
            });

            rateLimiterOptions.AddRedisTokenBucketLimiter("token-bucket", opt =>
            {
                opt.ConnectionMultiplexerFactory = () =>
                    services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
                opt.TokenLimit = 20;
                opt.TokensPerPeriod = 5;
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            });

            rateLimiterOptions.AddRedisConcurrencyLimiter("per-ip", opt =>
            {
                opt.ConnectionMultiplexerFactory = () =>
                    services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
                opt.PermitLimit = 5;
            });
        });

        return services;
    }
}
