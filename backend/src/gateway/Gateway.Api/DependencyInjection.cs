using Gateway.Api.Cache;
using Gateway.Api.Clients;
using Gateway.Api.Options;
using Gateway.Api.Services;
using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure.Time;
using SharedKernel.Time;

namespace Gateway.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        services.AddOptions<AuthServiceClientOptions>()
            .BindConfiguration(AuthServiceClientOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient(AuthServiceClientOptions.SectionName, (sp, client) =>
        {
            AuthServiceClientOptions options = sp.GetRequiredService<IOptions<AuthServiceClientOptions>>().Value;
            
            client.BaseAddress = options.BaseUrl;
        });

        services.AddScoped<IAuthServiceClient, AuthServiceClient>();

        services.AddScoped<IGatewayAuthService, GatewayAuthService>();

        services.AddSingleton<IAccessTokenCache, AccessTokenCache>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
