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
            options.Assemblies = [typeof(DependencyInjection).Assembly];
            options.PipelineBehaviors = [typeof(ValidationPipeline<,>), typeof(LoggingPipeline<,>)];
        });

        return services;
    }
    
    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        return services;
    }
}