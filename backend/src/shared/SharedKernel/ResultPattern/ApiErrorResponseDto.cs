using System.Text.Json.Serialization;

namespace SharedKernel.ResultPattern;

public sealed class ApiErrorResponseDto
{
    public string? Type { get; init; }

    public string? Title { get; init; }

    public string? Detail { get; init; }

    public string? Instance { get; init; }

    public int? Status { get; init; }

    [JsonExtensionData]
    public Dictionary<string, object?>? Extensions { get; init; }
}
