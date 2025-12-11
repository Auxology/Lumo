using SharedKernel.Api.Infrastructure;

namespace Auth.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        services.AddProblemDetails();

        return services;
    }
}