using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Infrastructure.Options;

using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

internal static class CachingSetup
{
    internal static IServiceCollection AddValkeySetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ValkeyOptions>()
            .Bind(configuration.GetSection(ValkeyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        ValkeyOptions valkeyOptions = new();
        configuration.GetSection(ValkeyOptions.SectionName).Bind(valkeyOptions);

        if (!valkeyOptions.Enabled)
            return services;

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(valkeyOptions.ConnectionString));

        services.AddStackExchangeRedisCache(cacheOptions =>
        {
            cacheOptions.Configuration = valkeyOptions.ConnectionString;
        });

        return services;
    }
}