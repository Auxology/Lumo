using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using SharedKernel.Infrastructure.Logging.Enrichers;

namespace SharedKernel.Infrastructure.Logging;

public static class SerilogConfigurationExtensions
{
    public static LoggerConfiguration ConfigureStandardEnrichers
    (
        this LoggerConfiguration configuration,
        IServiceProvider serviceProvider
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        return configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithClientIp()
            .Enrich.WithCorrelationId()
            .Enrich.WithRequestHeader("User-Agent")
            .Enrich.With(serviceProvider.GetRequiredService<RequestContextEnricher>())
            .Enrich.With(serviceProvider.GetRequiredService<UserContextEnricher>());
    }
}