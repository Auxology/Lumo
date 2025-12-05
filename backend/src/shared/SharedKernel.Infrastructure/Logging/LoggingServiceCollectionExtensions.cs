using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Context;
using SharedKernel.Infrastructure.Context;
using SharedKernel.Infrastructure.Logging.Enrichers;
using SharedKernel.Infrastructure.Options;

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

    public static SeqOptions GetSeqOptions(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        SeqOptions options = new();
        
        configuration.GetSection(SeqOptions.SectionName).Bind(options);
        
        return options;
    }
}