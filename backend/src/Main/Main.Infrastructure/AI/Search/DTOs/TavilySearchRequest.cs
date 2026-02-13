using System.Text.Json.Serialization;

namespace Main.Infrastructure.AI.Search.DTOs;

internal sealed record TavilySearchRequest
{
    [JsonPropertyName("query")]
    public required string Query { get; init; }

    [JsonPropertyName("topic")]
    public string Topic { get; init; } = "general";

    [JsonPropertyName("search_depth")]
    public required string SearchDepth { get; init; }

    [JsonPropertyName("max_results")]
    public required int MaxResults { get; init; }

    [JsonPropertyName("include_answer")]
    public bool IncludeAnswer { get; init; }
}