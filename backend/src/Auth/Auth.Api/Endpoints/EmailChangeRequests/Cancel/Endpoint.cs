using Auth.Application.Commands.EmailChangeRequests.Cancel;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.EmailChangeRequests.Cancel;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/api/email-change-requests/{RequestId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Cancel Email Change")
                .WithDescription("Cancels a pending email change request.")
                .Produces(204)
                .ProducesProblemDetails(401, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.EmailChangeRequests);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        CancelEmailChangeCommand command = new
        (
            RequestId: endpointRequest.RequestId
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}