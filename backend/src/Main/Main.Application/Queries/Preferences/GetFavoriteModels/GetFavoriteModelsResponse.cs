namespace Main.Application.Queries.Preferences.GetFavoriteModels;

public sealed record GetFavoriteModelsResponse(IReadOnlyList<FavoriteModelReadModel> FavoriteModels);