using FastEndpoints;

using Gateway.Api.Authentication;
using Gateway.Api.Extensions;

using SharedKernel;
using SharedKernel.Api.Constants;
using SharedKernel.Api.Infrastructure;

namespace Gateway.Api.Endpoints.Sessions.Logout;

internal sealed class Endpoint : EndpointWithoutRequest
{
    private readonly ISessionTokenOrchestrator _sessionTokenOrchestrator;

    public Endpoint(ISessionTokenOrchestrator sessionTokenOrchestrator)
    {
        _sessionTokenOrchestrator = sessionTokenOrchestrator;
    }

    public override void Configure()
    {
        Delete("/api/sessions");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Logout")
                .WithDescription("Logs out the current session by invalidating the refresh token cookie.")
                .Produces(204)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Sessions);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? refreshToken = HttpContext.GetRefreshTokenCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        Outcome outcome = await _sessionTokenOrchestrator.LogoutAsync(refreshToken, ct);

        if (outcome.IsFailure)
        {
            await Send.ResultAsync(CustomResults.Problem(outcome, HttpContext));
            return;
        }

        HttpContext.RemoveRefreshTokenCookie();

        await Send.NoContentAsync(ct);
    }
}