using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Infrastructure.Options;

public sealed class SerilogOptions
{
    public const string SectionName = "Serilog";
    
    [Required]
    public string MinimumLevel { get; init; } = "Information";

    public SeqOptions Seq { get; init; } = new();
    
    public ConsoleOptions Console { get; init; } = new();

    public Dictionary<string, string> OverrideMinimumLevels { get; init; } = new()
    {
        ["Microsoft"] = "Warning",
        ["Microsoft.Hosting.Lifetime"] = "Information",
        ["System"] = "Warning"
    };
}

public sealed class SeqOptions
{
    [Required, Url]
    public string ServerUrl { get; init; } = "http://localhost:5341";

    public string? ApiKey { get; init; }

    public bool Enabled { get; init; } = true;

    [Url]
    public string? HealthCheckUrl { get; init; }
}

public sealed class ConsoleOptions
{
    public bool Enabled { get; init; } = true;


    public string OutputTemplate { get; init; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
}