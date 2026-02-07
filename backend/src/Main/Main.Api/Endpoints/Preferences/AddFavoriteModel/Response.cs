namespace Main.Api.Endpoints.Preferences.AddFavoriteModel;

internal sealed record Response
(
    string PreferenceId,
    string FavoriteModelId,
    DateTimeOffset CreatedAt
);