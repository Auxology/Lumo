using FastEndpoints;

using Main.Application.Queries.Memories.GetUsage;

using Mediator;

using SharedKernel.Api.Infrastructure;

namespace Main.Api.Endpoints.Memories.GetUsage;

internal sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Get("/api/memories/usage");
        Version(1);

        Description(d => d
            .WithSummary("Get Memory Usage")
            .WithDescription("Returns current memory count and limit for the authenticated user.")
            .Produces<Response>(200, "application/json")
            .WithTags(CustomTags.Memories));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var outcome = await _sender.Send(new GetMemoryUsageQuery(), ct);

        if (outcome.IsFailure)
        {
            await Send.ResultAsync(CustomResults.Problem(outcome, HttpContext));
            return;
        }

        Response response = new
        (
            CurrentCount: outcome.Value.CurrentCount,
            MaxCount: outcome.Value.MaxCount,
            IsAtLimit: outcome.Value.IsAtLimit
        );

        await Send.ResponseAsync(response, cancellation: ct);
    }
}