using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    [Required, MinLength(1)]
    public string ServiceName { get; init; } = string.Empty;

    public string? ServiceVersion { get; init; }

    public string? ServiceNamespace { get; init; }

    public OtelExporterOptions Exporter { get; init; } = new();

    public TracingOptions Tracing { get; init; } = new();

    public MetricsOptions Metrics { get; init; } = new();
}

public sealed class OtelExporterOptions
{
    [Required, Url]
    public string Endpoint { get; init; } = "http://localhost:4317";

    public bool Enabled { get; init; } = true;
}

public sealed class TracingOptions
{
    public bool Enabled { get; init; } = true;

    public bool RecordException { get; init; } = true;

    public InstrumentationOptions Instrumentation { get; init; } = new();
}

public sealed class MetricsOptions
{
    public bool Enabled { get; init; } = true;

    public InstrumentationOptions Instrumentation { get; init; } = new();
}

public sealed class InstrumentationOptions
{
    public bool AspNetCore { get; init; } = true;
    public bool HttpClient { get; init; } = true;
    public bool SqlClient { get; init; } = true;
    public bool EntityFrameworkCore { get; init; } = true;
    public bool Runtime { get; init; } = true;
    public bool Process { get; init; } = true;
}