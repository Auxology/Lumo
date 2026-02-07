using Auth.Application.Commands.Sessions.RevokeSessions;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.Sessions.RevokeSessions;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Delete("/api/sessions/batch");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Revoke Sessions")
                .WithDescription("Revokes multiple sessions for the authenticated user. The current session cannot be revoked.")
                .Produces(204)
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        RevokeSessionsCommand command = new(SessionIds: endpointRequest.SessionIds);

        await SendOutcomeAsync(
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}