using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SharedKernel.Infrastructure.Logging.Enrichers;
using SharedKernel.Infrastructure.Options;

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
    
    public static LoggerConfiguration ConfigureSinks
    (
        this LoggerConfiguration configuration,
        IHostEnvironment environment,
        SeqOptions? seqOptions = null
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        if (environment.IsDevelopment())
        {
            configuration.WriteTo.Console
            (
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug,
                formatProvider: CultureInfo.InvariantCulture
            );
        }
        
        if (seqOptions?.IsEnabled == true)
        {
            configuration.WriteTo.Seq
            (
                serverUrl: seqOptions.ServerUrl.AbsoluteUri,
                apiKey: seqOptions.ApiKey,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                formatProvider: CultureInfo.InvariantCulture
            );
        }
        
        return configuration;
    }
}