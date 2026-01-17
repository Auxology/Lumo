using Auth.Application.Commands.RecoveryRequests.VerifyNewEmail;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.RecoveryRequests.Verify;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/recovery-requests/{TokenKey}/verify");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Verify New Email for Recovery")
                .WithDescription("Verifies ownership of the new email address via OTP or magic link.")
                .Produces(204)
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .WithTags(CustomTags.Recovery);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        VerifyNewEmailCommand command = new
        (
            TokenKey: endpointRequest.TokenKey,
            OtpToken: endpointRequest.OtpToken,
            MagicLinkToken: endpointRequest.MagicLinkToken
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}