using FastEndpoints;

using Main.Application.Commands.Memories.Delete;

using Mediator;

using SharedKernel;
using SharedKernel.Api.Infrastructure;

namespace Main.Api.Endpoints.Memories.Delete;

internal sealed class Endpoint : EndpointWithoutRequest
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/api/memories");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Delete All Memories")
                .WithDescription("Permanently deletes all memories for the authenticated user.")
                .Produces(204)
                .ProducesProblemDetails(401, "application/json")
                .WithTags(CustomTags.Memories);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        DeleteMemoriesCommand command = new();

        Outcome outcome = await _sender.Send(command, ct);

        if (outcome.IsFailure)
        {
            await Send.ResultAsync(CustomResults.Problem(outcome, HttpContext));
            return;
        }

        await Send.NoContentAsync(ct);
    }
}