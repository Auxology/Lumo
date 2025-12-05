using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Context;
using SharedKernel.Infrastructure.Context;
using SharedKernel.Infrastructure.Logging.Enrichers;

namespace SharedKernel.Infrastructure.Logging;

public static class LoggingServiceCollectionExtensions
{
  
    public static IServiceCollection AddLoggingInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IRequestContext, RequestContext>();

        services.AddScoped<RequestContextEnricher>();
        services.AddScoped<UserContextEnricher>();

        return services;
    }
}