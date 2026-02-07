using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Preferences.GetFavoriteModels;

public sealed record GetFavoriteModelsQuery : IQuery<GetFavoriteModelsResponse>;