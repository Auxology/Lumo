using Auth.Application.Commands.Sessions.Logout;

using Contracts.Requests;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

namespace Auth.Api.Endpoints.Sessions.Logout;

internal sealed class Endpoint : BaseEndpoint<LogoutApiRequest>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/api/sessions");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Logout")
                .WithDescription("Logs out the current session by invalidating the refresh token.")
                .Produces(204)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(LogoutApiRequest endpointRequest, CancellationToken ct)
    {
        LogoutCommand command = new
        (
            RefreshToken: endpointRequest.RefreshToken
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}