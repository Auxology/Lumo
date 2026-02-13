using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Application.Pipelines;

namespace Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddMessaging()
            .AddFluentValidation();

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationPipeline<,>), typeof(LoggingPipeline<,>)];
        });

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}