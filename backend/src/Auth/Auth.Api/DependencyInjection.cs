using Auth.Api.Options;
using FastEndpoints;
using FastEndpoints.Swagger;
using SharedKernel.Api;

namespace Auth.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddAuthApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedKernelApi();

        services.AddOptions<AuthApiOptions>()
            .Bind(configuration.GetSection(AuthApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        AuthApiOptions authApiOptions = new();
        configuration.GetSection(AuthApiOptions.SectionName).Bind(authApiOptions);

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
                s.Title = authApiOptions.Title;
                s.Description = authApiOptions.Description;
                s.Version = authApiOptions.Version;
            };
        });

        return services;
    }
}
