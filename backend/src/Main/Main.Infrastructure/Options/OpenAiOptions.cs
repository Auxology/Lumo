using System.ComponentModel.DataAnnotations;

namespace Main.Infrastructure.Options;

internal sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    [Required, MinLength(1)]
    public string ApiKey { get; init; } = string.Empty;

    public string EmbeddingModel { get; init; } = "text-embedding-3-small";
}