using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Api.Infrastructure;

namespace SharedKernel.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedApiInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}

