using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Authentication;
using SharedKernel.Infrastructure.Authentication;
using SharedKernel.Infrastructure.Caching;
using SharedKernel.Infrastructure.Observability;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Infrastructure.Time;

namespace SharedKernel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernelInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddServices()
            .AddAuthenticationInternal(configuration)
            .AddOpenTelemetrySetup(configuration)
            .AddValkeySetup(configuration);

        return services;
    }
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services,
        IConfiguration configuration)

    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddSharedHealthChecks(this IServiceCollection services,
        IConfiguration configuration)

    {
        ArgumentNullException.ThrowIfNull(configuration);

        ValkeyOptions valkeyOptions = new ValkeyOptions();
        configuration.GetSection(ValkeyOptions.SectionName).Bind(valkeyOptions);

        SerilogOptions serilogOptions = new SerilogOptions();
        configuration.GetSection(SerilogOptions.SectionName).Bind(serilogOptions);

        OpenTelemetryOptions openTelemetryOptions = new OpenTelemetryOptions();
        configuration.GetSection(OpenTelemetryOptions.SectionName).Bind(openTelemetryOptions);

        string seqHealthUrl = serilogOptions.Seq.HealthCheckUrl ?? serilogOptions.Seq.ServerUrl + "/api";

        string jaegerHealthUrl = openTelemetryOptions.Exporter.HealthCheckUrl ?? openTelemetryOptions.Exporter.Endpoint;

        services.AddHealthChecks()
            .AddRedis
            (
                redisConnectionString: valkeyOptions.ConnectionString,
                name: "valkey",
                tags: ["ready"]
            )
            .AddUrlGroup
            (
                new Uri(seqHealthUrl),
                name: "seq",
                tags: ["ready"]
            )
            .AddUrlGroup
            (
                new Uri(jaegerHealthUrl),
                name: "jaeger",
                tags: ["ready"]
            );

        return services;
    }
}