namespace Main.Api.Endpoints.Preferences.GetFavoriteModels;

internal sealed record FavoriteModelDto
(
    string Id,
    string ModelId,
    string DisplayName,
    string Provider,
    bool IsDefault,
    int MaxContextTokens,
    bool SupportsVision,
    bool SupportsStreaming,
    DateTimeOffset CreatedAt
);

internal sealed record Response(IReadOnlyList<FavoriteModelDto> FavoriteModels);