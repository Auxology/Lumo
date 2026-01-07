using System.Globalization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;

using SharedKernel.Infrastructure.Options;

namespace SharedKernel.Infrastructure.Observability;

public static class SerilogSetup
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            SerilogOptions serilogOptions = new();
            context.Configuration.GetSection(SerilogOptions.SectionName).Bind(serilogOptions);

            ConfigureSerilog(configuration, serilogOptions, context.HostingEnvironment);
        });
    }

    private static void ConfigureSerilog
    (
        LoggerConfiguration configuration,
        SerilogOptions options,
        IHostEnvironment environment
    )
    {
        LogEventLevel minimumLevel = ParseLogLevel(options.MinimumLevel);

        configuration
            .MinimumLevel.Is(minimumLevel)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithSpan()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", environment.ApplicationName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName);

        foreach ((string source, string level) in options.OverrideMinimumLevels)
        {
            configuration.MinimumLevel.Override(source, ParseLogLevel(level));
        }

        if (options.Console.Enabled)
        {
            configuration.WriteTo.Console
            (
                outputTemplate: options.Console.OutputTemplate,
                restrictedToMinimumLevel: minimumLevel,
                formatProvider: CultureInfo.InvariantCulture
            );
        }

        if (options.Seq.Enabled)
        {
            configuration.WriteTo.Seq
            (
                serverUrl: options.Seq.ServerUrl,
                apiKey: options.Seq.ApiKey,
                restrictedToMinimumLevel: minimumLevel,
                formatProvider: CultureInfo.InvariantCulture
            );
        }
    }

    private static LogEventLevel ParseLogLevel(string level) =>
        Enum.TryParse<LogEventLevel>(level, ignoreCase: true, out var result)
            ? result
            : LogEventLevel.Information;
}