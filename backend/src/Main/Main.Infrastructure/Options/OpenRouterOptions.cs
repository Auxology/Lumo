using System.ComponentModel.DataAnnotations;

namespace Main.Infrastructure.Options;

internal sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    [Required, MinLength(1)]
    public string ApiKey { get; init; } = string.Empty;

    [Required, Url]
    public string BaseUrl { get; init; } = "https://openrouter.ai/api/v1";

    [Required, MinLength(1)]
    public string DefaultModel { get; init; } = "allenai/olmo-3.1-32b-think:free";

    public string? AppName { get; init; }

    [Url]
    public string? SiteUrl { get; init; }

    public Dictionary<string, string> ModelMappings { get; init; } = new()
    {
        ["allenai/olmo-3.1"] = "allenai/olmo-3.1-32b-think:free"
    };
}