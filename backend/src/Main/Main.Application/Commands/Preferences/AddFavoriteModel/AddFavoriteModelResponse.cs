namespace Main.Application.Commands.Preferences.AddFavoriteModel;

public sealed record AddFavoriteModelResponse
(
    string PreferenceId,
    string FavoriteModelId,
    DateTimeOffset CreatedAt
);