using System.ComponentModel.DataAnnotations;

namespace Main.Api.Options;

internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; init; } = true;

    [Range(1, 100)]
    public int AiGenerationTokenLimit { get; init; } = 10;

    [Range(1, 50)]
    public int AiGenerationTokensPerPeriod { get; init; } = 2;

    [Range(typeof(TimeSpan), "00:00:05", "00:10:00")]
    public TimeSpan AiGenerationReplenishmentPeriod { get; init; } = TimeSpan.FromSeconds(30);
}