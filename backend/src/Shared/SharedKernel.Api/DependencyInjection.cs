using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Api.Infrastructure;

namespace SharedKernel.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernelApi(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}