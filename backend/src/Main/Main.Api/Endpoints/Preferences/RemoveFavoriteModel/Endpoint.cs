using FastEndpoints;

using Main.Application.Commands.Preferences.RemoveFavoriteModel;

using Mediator;

using SharedKernel.Api.Constants;

namespace Main.Api.Endpoints.Preferences.RemoveFavoriteModel;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/api/preferences/favorite-models/{favoriteModelId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Remove Favorite Model")
                .WithDescription("Removes a model from the user's favorites.")
                .Produces(204)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(404, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Preferences);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        RemoveFavoriteModelCommand command = new(FavoriteModelId: request.FavoriteModelId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}