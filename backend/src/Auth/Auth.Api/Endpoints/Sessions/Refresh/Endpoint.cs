using Auth.Application.Sessions;
using FastEndpoints;
using Mediator;

namespace Auth.Api.Endpoints.Sessions.Refresh;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/sessions/refresh");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Refresh Session")
                .WithDescription("Refreshes an expired access token using a valid refresh token.")
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        RefreshTokenCommand command = new
        (
            RefreshToken: endpointRequest.RefreshToken
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: rtr => new Response
            (
                AccessToken: rtr.AccessToken,
                RefreshToken: rtr.RefreshToken
            ),
            successStatusCode: 200,
            ct
        );
    }
}
