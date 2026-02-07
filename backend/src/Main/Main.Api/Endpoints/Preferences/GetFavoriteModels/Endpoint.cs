using FastEndpoints;

using Main.Application.Queries.Preferences.GetFavoriteModels;

using Mediator;

using SharedKernel.Api.Infrastructure;

namespace Main.Api.Endpoints.Preferences.GetFavoriteModels;

internal sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/preferences/favorite-models");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Favorite Models")
                .WithDescription("Retrieves all favorite models for the authenticated user.")
                .Produces<Response>(200, "application/json")
                .WithTags(CustomTags.Preferences);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        GetFavoriteModelsQuery query = new();

        var outcome = await _sender.Send(query, ct);

        if (outcome.IsFailure)
        {
            await Send.ResultAsync(CustomResults.Problem(outcome, HttpContext));
            return;
        }

        Response response = new
        (
            FavoriteModels: outcome.Value.FavoriteModels
                .Select(f => new FavoriteModelDto
                (
                    Id: f.Id,
                    ModelId: f.ModelId,
                    DisplayName: f.DisplayName,
                    Provider: f.Provider,
                    IsDefault: f.IsDefault,
                    MaxContextTokens: f.MaxContextTokens,
                    SupportsVision: f.SupportsVision,
                    SupportsStreaming: f.SupportsStreaming,
                    CreatedAt: f.CreatedAt
                ))
                .ToList()
        );

        await Send.ResponseAsync(response, cancellation: ct);
    }
}