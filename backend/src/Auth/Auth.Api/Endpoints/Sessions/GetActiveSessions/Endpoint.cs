using Auth.Application.Queries.Sessions.GetActiveSessions;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.Sessions.GetActiveSessions;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Get("/api/sessions");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Active Sessions")
                .WithDescription("Returns all active sessions for the authenticated user.")
                .Produces<Response>(200)
                .ProducesProblemDetails(401)
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(EmptyRequest endpointRequest, CancellationToken ct)
    {
        GetActiveSessionsQuery query = new();

        await SendOutcomeAsync(
            outcome: await _sender.Send(query, ct),
            mapper: sessions => new Response
            (
                Sessions: sessions.Select(s => new SessionDto
                (
                    Id: s.Id,
                    CreatedAt: s.CreatedAt,
                    ExpiresAt: s.ExpiresAt,
                    LastRefreshAt: s.LastRefreshAt,
                    IpAddress: s.IpAddress,
                    Browser: s.NormalizedBrowser,
                    Os: s.NormalizedOs,
                    IsCurrent: s.IsCurrent
                )).ToList()
            ),
            cancellationToken: ct
        );
    }
}