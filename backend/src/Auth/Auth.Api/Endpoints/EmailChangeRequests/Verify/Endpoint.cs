using Auth.Application.Commands.EmailChangeRequests.Verify;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.EmailChangeRequests.Verify;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/email-change-requests/{TokenKey}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Verify Email Change")
                .WithDescription("Verifies the OTP and completes the email change by updating the request state.")
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .ProducesProblemDetails(409, "application/json")
                .WithTags(CustomTags.EmailChangeRequests);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        VerifyEmailChangeCommand command = new
        (
            TokenKey: endpointRequest.TokenKey,
            OtpToken: endpointRequest.OtpToken
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: vecr => new Response(NewEmailAddress: vecr.NewEmailAddress),
            cancellationToken: ct
        );
    }
}