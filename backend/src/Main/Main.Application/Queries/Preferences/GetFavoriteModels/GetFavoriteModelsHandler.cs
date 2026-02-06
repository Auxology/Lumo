using System.Data.Common;

using Dapper;

using Main.Application.Abstractions.AI;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;
using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Preferences.GetFavoriteModels;

internal sealed class GetFavoriteModelsHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext,
    IModelRegistry modelRegistry) : IQueryHandler<GetFavoriteModelsQuery, GetFavoriteModelsResponse>
{
    private const string Sql = """
                               SELECT
                                    fm.Id as Id,
                                    fm.model_id as ModelId,
                                    fm.created_at as CreatedAt
                               FROM favorite_models fm
                               INNER JOIN preferences p ON p.id = fm.preference_id
                               WHERE p.user_id = @UserId
                               ORDER BY fm.created_at DESC
                               """;

    public async ValueTask<Outcome<GetFavoriteModelsResponse>> Handle(GetFavoriteModelsQuery request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        IEnumerable<FavoriteModelDbRow> favorites = await connection.QueryAsync<FavoriteModelDbRow>
        (
            Sql,
            new { UserId = userId }
        );

        List<FavoriteModelReadModel> models = favorites
            .Select(f =>
            {
                ModelInfo? info = modelRegistry.GetModelInfo(f.ModelId);

                return new FavoriteModelReadModel
                {
                    Id = f.Id,
                    ModelId = f.ModelId,
                    DisplayName = info?.DisplayName ?? f.ModelId,
                    Provider = info?.Provider ?? "Unknown Provider",
                    IsDefault = info?.IsDefault ?? false,
                    MaxContextTokens = info?.ModelCapabilities.MaxContextTokens ?? 0,
                    SupportsVision = info?.ModelCapabilities.SupportsVision ?? false,
                    SupportsStreaming = info?.ModelCapabilities.SupportsStreaming ?? false,
                    CreatedAt = f.CreatedAt
                };
            })
            .ToList();

        GetFavoriteModelsResponse response = new(models);

        return response;
    }
}