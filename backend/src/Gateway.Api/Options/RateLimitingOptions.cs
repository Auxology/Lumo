using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Options;

internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; init; } = true;

    [Range(1, 10000)]
    public int GlobalPermitLimit { get; init; } = 100;

    [Range(1, 3600)]
    public int GlobalWindowSeconds { get; init; } = 60;

    [Range(1, 1000)]
    public int AuthPermitLimit { get; init; } = 10;

    [Range(1, 3600)]
    public int AuthWindowSeconds { get; init; } = 60;

    [Range(1, 1000)]
    public int ChatPermitLimit { get; init; } = 30;

    [Range(1, 3600)]
    public int ChatWindowSeconds { get; init; } = 60;

    [Range(1, 100)]
    public int MessageTokenLimit { get; init; } = 10;

    [Range(1, 100)]
    public int MessageTokensPerPeriod { get; init; } = 2;

    [Range(typeof(TimeSpan), "00:00:01", "00:10:00")]
    public TimeSpan MessageReplenishmentPeriod { get; init; } = TimeSpan.FromSeconds(10);
}