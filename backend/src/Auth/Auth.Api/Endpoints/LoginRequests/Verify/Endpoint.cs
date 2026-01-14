using Auth.Application.Commands.LoginRequests.Verify;

using Contracts.Requests;
using Contracts.Responses;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.LoginRequests.Verify;

internal sealed class Endpoint : BaseEndpoint<VerifyLoginApiRequest, VerifyLoginApiResponse>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/login-requests/verify");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Verify Login Request")
                .WithDescription("Verifies a login request using OTP or magic link token and returns session tokens.")
                .Produces<VerifyLoginApiResponse>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .WithTags(CustomTags.LoginRequests);
        });
    }

    public override async Task HandleAsync(VerifyLoginApiRequest endpointRequest, CancellationToken ct)
    {
        VerifyLoginCommand command = new
        (
            TokenKey: endpointRequest.TokenKey,
            OtpToken: endpointRequest.OtpToken,
            MagicLinkToken: endpointRequest.MagicLinkToken
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: vlr => new VerifyLoginApiResponse
                (
                    AccessToken: vlr.AccessToken,
                    RefreshToken: vlr.RefreshToken
                ),
            successStatusCode: 200,
            ct
        );
    }
}