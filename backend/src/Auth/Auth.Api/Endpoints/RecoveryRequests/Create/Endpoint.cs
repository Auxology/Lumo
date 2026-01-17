using Auth.Application.Commands.RecoveryRequests.Initiate;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.RecoveryRequests.Create;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/recovery-requests");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Create Recovery Request")
                .WithDescription("Validates recovery key and sends OTP to new email address.")
                .Produces<Response>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .ProducesProblemDetails(409, "application/json")
                .WithTags(CustomTags.Recovery);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        InitiateRecoveryCommand command = new
        (
            RecoveryKey: endpointRequest.RecoveryKey,
            NewEmailAddress: endpointRequest.NewEmailAddress
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                TokenKey: r.TokenKey,
                RemainingRecoveryKeys: r.RemainingRecoveryKeys
            ),
            successStatusCode: 201,
            ct
        );
    }
}