using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SharedKernel.Infrastructure.Options;

namespace SharedKernel.Infrastructure.Observability;

public static class OpenTelemetrySetup
{
    public static IServiceCollection AddOpenTelemetrySetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<OpenTelemetryOptions>()
            .Bind(configuration.GetSection(OpenTelemetryOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        OpenTelemetryOptions options = new();
        configuration.GetSection(OpenTelemetryOptions.SectionName).Bind(options);

        ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService
            (
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion,
                serviceNamespace: options.ServiceNamespace
            );

        if (options.Tracing.Enabled)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder => ConfigureTracing(builder, options, resourceBuilder));
        }

        if (options.Metrics.Enabled)
        {
            services.AddOpenTelemetry()
                .WithMetrics(builder => ConfigureMetrics(builder, options, resourceBuilder));
        }

        return services;
    }

    private static void ConfigureTracing
    (
        TracerProviderBuilder builder,
        OpenTelemetryOptions options,
        ResourceBuilder resourceBuilder
    )
    {
        builder.SetResourceBuilder(resourceBuilder);

        InstrumentationOptions instrumentation = options.Tracing.Instrumentation;

        if (instrumentation.AspNetCore)
        {
            builder.AddAspNetCoreInstrumentation(opts =>
            {
                opts.RecordException = options.Tracing.RecordException;
            });
        }

        if (instrumentation.HttpClient)
        {
            builder.AddHttpClientInstrumentation(opts =>
            {
                opts.RecordException = options.Tracing.RecordException;
            });
        }

        if (instrumentation.SqlClient)
        {
            builder.AddSqlClientInstrumentation(opts =>
            {
                opts.RecordException = options.Tracing.RecordException;
            });
        }

        if (instrumentation.EntityFrameworkCore)
        {
            builder.AddEntityFrameworkCoreInstrumentation();
        }

        if (options.Exporter.Enabled)
        {
            builder.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(options.Exporter.Endpoint);
            });
        }
    }

    private static void ConfigureMetrics
    (
        MeterProviderBuilder builder,
        OpenTelemetryOptions options,
        ResourceBuilder resourceBuilder
    )
    {
        builder.SetResourceBuilder(resourceBuilder);

        InstrumentationOptions instrumentation = options.Metrics.Instrumentation;

        if (instrumentation.AspNetCore)
        {
            builder.AddAspNetCoreInstrumentation();
        }

        if (instrumentation.HttpClient)
        {
            builder.AddHttpClientInstrumentation();
        }

        if (instrumentation.Runtime)
        {
            builder.AddRuntimeInstrumentation();
        }

        if (instrumentation.Process)
        {
            builder.AddProcessInstrumentation();
        }

        if (options.Exporter.Enabled)
        {
            builder.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(options.Exporter.Endpoint);
            });
        }
    }
}
