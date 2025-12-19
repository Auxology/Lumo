using Gateway.Api.Caching;
using SharedKernel.Api;
using SharedKernel.Infrastructure;

namespace Gateway.Api;

internal static class DependencyInjection
{
    internal static IServiceCollection AddGatewayApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOpenApi()
            .AddSharedKernelApi()
            .AddSharedKernelInfrastructure(configuration)
            .AddSharedHealthChecks(configuration);

        services.AddScoped<ITokenCacheService, TokenCacheService>();

        return services;
    }
}