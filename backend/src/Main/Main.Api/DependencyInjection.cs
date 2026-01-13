using FastEndpoints;
using FastEndpoints.Swagger;

using Main.Api.Options;
using Main.Api.RateLimiting;

using SharedKernel.Api;

namespace Main.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddMainApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSharedKernelApi()
            .AddRateLimitingSetup(configuration);

        services.AddOptions<MainApiOptions>()
            .Bind(configuration.GetSection(MainApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        MainApiOptions mainApiOptions = new();
        configuration.GetSection(MainApiOptions.SectionName).Bind(mainApiOptions);

        services.AddFastEndpoints(options =>
        {
            options.Assemblies =
            [
                typeof(DependencyInjection).Assembly
            ];
        });

        services.SwaggerDocument(o =>
        {
            o.MaxEndpointVersion = 1;
            o.DocumentSettings = s =>
            {
                s.Title = mainApiOptions.Title;
                s.Description = mainApiOptions.Description;
                s.Version = mainApiOptions.Version;
            };
        });

        return services;
    }
}