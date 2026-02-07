namespace Main.Application.Queries.Preferences.GetFavoriteModels;

public sealed record FavoriteModelReadModel
{
    public required string Id { get; init; }

    public required string ModelId { get; init; }

    public required string DisplayName { get; init; }

    public required string Provider { get; init; }

    public bool IsDefault { get; init; }

    public int MaxContextTokens { get; init; }

    public bool SupportsVision { get; init; }

    public bool SupportsStreaming { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}