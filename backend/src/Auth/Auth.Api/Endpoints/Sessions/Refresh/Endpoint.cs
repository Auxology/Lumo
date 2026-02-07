using Auth.Application.Commands.Sessions.RefreshToken;

using Contracts.Requests;
using Contracts.Responses;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

namespace Auth.Api.Endpoints.Sessions.Refresh;

internal sealed class Endpoint : BaseEndpoint<RefreshSessionApiRequest, RefreshSessionApiResponse>
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
                .Produces<RefreshSessionApiResponse>(200, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(RefreshSessionApiRequest endpointRequest, CancellationToken ct)
    {
        RefreshTokenCommand command = new
        (
            RefreshToken: endpointRequest.RefreshToken
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: rtr => new RefreshSessionApiResponse
            (
                AccessToken: rtr.AccessToken,
                RefreshToken: rtr.RefreshToken
            ),
            successStatusCode: 200,
            ct
        );
    }
}