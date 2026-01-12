using Auth.Application.Queries.Sessions.GetActiveSessionCount;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.Sessions.GetActiveSessionCount;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Get("/api/sessions/count");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Active Session Count")
                .WithDescription("Returns the count of active sessions for the authenticated user.")
                .Produces<Response>(200)
                .ProducesProblemDetails(401)
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        GetActiveSessionCountQuery query = new();

        await SendOutcomeAsync(
            outcome: await _sender.Send(query, ct),
            mapper: r => new Response(r),
            cancellationToken: ct
        );
    }
}