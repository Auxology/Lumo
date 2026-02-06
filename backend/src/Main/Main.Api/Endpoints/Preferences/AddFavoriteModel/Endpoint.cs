using FastEndpoints;

using Main.Application.Commands.Preferences.AddFavoriteModel;

using Mediator;

namespace Main.Api.Endpoints.Preferences.AddFavoriteModel;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/preferences/favorite-models");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Add Favorite Model")
                .WithDescription("Adds a model to the user's favorites.")
                .Produces<Response>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(409, "application/json")
                .WithTags(CustomTags.Preferences);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        AddFavoriteModelCommand command = new(ModelId: request.ModelId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                PreferenceId: r.PreferenceId,
                FavoriteModelId: r.FavoriteModelId,
                CreatedAt: r.CreatedAt
            ),
            successStatusCode: 201,
            cancellationToken: ct
        );
    }
}