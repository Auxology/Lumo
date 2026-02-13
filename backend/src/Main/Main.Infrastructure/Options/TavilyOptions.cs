using System.ComponentModel.DataAnnotations;

namespace Main.Infrastructure.Options;

internal sealed class TavilyOptions
{
    public const string SectionName = "Tavily";

    [Required, MinLength(1)]
    public string ApiKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public int MaxResults { get; init; } = 5;

    public string SearchDepth { get; init; } = string.Empty;
}