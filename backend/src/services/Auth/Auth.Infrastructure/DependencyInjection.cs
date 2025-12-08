using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Authentication;
using SharedKernel.Infrastructure.Authentication;
using SharedKernel.Infrastructure.DomainEvents;
using SharedKernel.Infrastructure.Logging;
using SharedKernel.Infrastructure.Pipelines;
using SharedKernel.Infrastructure.Time;
using SharedKernel.Time;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddLoggingInfrastructure()
            .AddInfraPipelines()
            .AddServices();
    
    private static IServiceCollection AddInfraPipelines(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        
        return services;
    }
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddSingleton<ISecretHasher, SecretHasher>();

        services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
    
}