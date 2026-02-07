using Contracts.Requests;

using FastEndpoints;

using Gateway.Api.Authentication;
using Gateway.Api.Extensions;

using SharedKernel;
using SharedKernel.Api.Constants;
using SharedKernel.Api.Infrastructure;

namespace Gateway.Api.Endpoints.LoginRequests.Verify;

internal sealed class Endpoint : Endpoint<VerifyLoginApiRequest>
{
    private readonly ISessionTokenOrchestrator _sessionTokenOrchestrator;

    public Endpoint(ISessionTokenOrchestrator sessionTokenOrchestrator)
    {
        _sessionTokenOrchestrator = sessionTokenOrchestrator;
    }

    public override void Configure()
    {
        Post("/api/login-requests/verify");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Verify Login")
                .WithDescription("Verifies login credentials and sets refresh token cookie.")
                .Produces(204)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.LoginRequests);
        });
    }

    public override async Task HandleAsync(VerifyLoginApiRequest endpointRequest, CancellationToken ct)
    {
        Outcome<string> outcome = await _sessionTokenOrchestrator.VerifyLoginAsync(endpointRequest, ct);

        if (outcome.IsFailure)
        {
            await Send.ResultAsync(CustomResults.Problem(outcome, HttpContext));
            return;
        }

        HttpContext.SetRefreshTokenCookie(outcome.Value);

        await Send.NoContentAsync(ct);
    }
}