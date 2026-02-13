using Auth.Application.Commands.EmailChangeRequests.Verify;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

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
        Patch("/api/email-change-requests/{RequestId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Verify Email Change")
                .WithDescription("Verifies the OTP and completes the email change by updating the request state.")
                .Produces<Response>(200, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(404, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(409, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.EmailChangeRequests);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        VerifyEmailChangeCommand command = new
        (
            RequestId: endpointRequest.RequestId,
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