namespace SharedKernel.Infrastructure.Options;

public sealed class SeqOptions
{
    public const string SectionName = "Seq";
    
    public Uri ServerUrl { get; init; } = new("http://localhost:5341");
    
    public string? ApiKey { get; init; }

    public bool IsEnabled { get; init; } = true;
}