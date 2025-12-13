using Gateway.Api.Extensions;
using Gateway.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Authentication;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Gateway.Api.Endpoints.Users;

internal sealed class CreateSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sessions", HandleAsync)
            .WithName("CreateSession")
            .WithTags(Tags.Authentication)
            .WithDescription("Public endpoint for completing passwordless authentication and establishing user sessions")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Complete passwordless login";
                operation.Description = """
                                        Verifies the user's identity using passwordless authentication and establishes a session.

                                        ## Authentication Flow
                                        This is the final step in passwordless authentication:
                                        1. User receives OTP code via email or magic link via email/SMS
                                        2. User submits credentials via this endpoint
                                        3. Gateway validates credentials with Auth service
                                        4. On success, session is established with token pair

                                        ## Authentication Options
                                        Provide **one** of the following with the user's email address:
                                        - `otpToken`: 6-digit numeric code sent via email
                                        - `magicLinkToken`: Single-use token from magic link URL

                                        ## Response Behavior
                                        On successful authentication:
                                        - Returns HTTP 201 Created with empty body
                                        - Sets `refreshToken` as secure HTTP-only cookie
                                        - Access token cached server-side and paired with refresh token
                                        - Client uses refresh token cookie for subsequent access token retrieval

                                        ## Security Features
                                        - Rate limiting applied per email address
                                        - Failed login attempts are logged and monitored
                                        - Account lockout after 5 consecutive failed attempts
                                        - Tokens are single-use and invalidated after verification
                                        - Refresh token stored as HTTP-only, Secure, SameSite cookie

                                        ## Error Responses
                                        - `400 Bad Request`: Invalid request format or missing required fields
                                        - `401 Unauthorized`: Invalid credentials or expired token
                                        - `429 Too Many Requests`: Rate limit exceeded for this email address
                                        """;
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync
    (
        [FromBody] CreateSessionRequest request,
        HttpContext httpContext,
        IGatewayAuthService authService,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken
    )
    {
        Result<string> result = await authService.VerifyLoginAsync(request, cancellationToken);

        if (result.IsFailure)
            return CustomResults.Problem(result, httpContext);

        string refreshToken = result.Value;

        httpContext.SetRefreshTokenCookie(refreshToken, dateTimeProvider);

        return Results.Created(new Uri("/api/sessions", UriKind.Relative), null);
    }
}
