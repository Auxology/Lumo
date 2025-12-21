using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Pipelines;

namespace Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddMediatR()
            .AddFluentValidation();
    
    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            
            cfg.AddOpenBehavior(typeof(ValidationPipeline<,>));
            cfg.AddOpenBehavior(typeof(LoggingPipeline<,>));
        });

        return services;
    }
    
    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        return services;
    }
}