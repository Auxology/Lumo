namespace Main.Application.Queries.Preferences.GetFavoriteModels;

public sealed record FavoriteModelDbRow
{
    public required string Id { get; init; }

    public required string ModelId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}