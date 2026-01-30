using System.Text.Json.Serialization;

namespace Main.Infrastructure.AI.Tools;

internal sealed record SaveMemoryArguments
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }
}