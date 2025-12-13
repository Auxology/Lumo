using Auth.Application.Users.VerifyLogin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Authentication;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class CreateSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/non-passing/api/sessions", HandleAsync)
            .WithName("CreateSession")
            .WithTags(Tags.Authentication)
            .WithDescription("Internal endpoint for verifying passwordless authentication credentials")
            .ExcludeFromDescription()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "[Internal] Verify login credentials and issue tokens";
                operation.Description = """
                                        **INTERNAL USE ONLY** - Called exclusively by the Gateway API during authentication flow.

                                        This endpoint handles the core authentication logic for passwordless login:
                                        - Validates OTP codes or magic link tokens
                                        - Verifies user identity and account status
                                        - Generates JWT access and refresh token pair
                                        - Returns tokens to Gateway for client session establishment

                                        ## Service Responsibility
                                        The Auth service is responsible for:
                                        - Token validation (OTP/magic link)
                                        - User verification and account status checks
                                        - JWT token pair generation
                                        - Failed attempt tracking and account lockout logic
                                        - Token invalidation after successful verification

                                        ## Gateway Integration
                                        The Gateway consumes this endpoint to:
                                        1. Forward client authentication requests
                                        2. Receive token pair (access + refresh)
                                        3. Cache access token server-side
                                        4. Set refresh token as HTTP-only cookie for client
                                        5. Return session establishment response to client

                                        ## Authentication Options
                                        Provide **one** of the following:
                                        - `otpToken`: 6-digit code for email-based verification
                                        - `magicLinkToken`: Single-use token from magic link

                                        ## Response
                                        Returns `VerifyLoginResponse` containing:
                                        - `accessToken`: Short-lived JWT for API authorization
                                        - `refreshToken`: Long-lived token for access token renewal

                                        ## Security Features
                                        - Single-use tokens (invalidated after verification)
                                        - Failed attempt logging per user
                                        - Account lockout after 5 consecutive failures
                                        - Token expiration validation
                                        - Rate limiting enforcement

                                        ## Error Responses
                                        - `400 Bad Request`: Validation errors or malformed request
                                        - `401 Unauthorized`: Invalid or expired credentials
                                        - `429 Too Many Requests`: Rate limit exceeded
                                        """;
                return Task.CompletedTask;
            })
            .Produces<VerifyLoginResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync
    (
        [FromBody] CreateSessionRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        VerifyLoginCommand command = new
            (
                EmailAddress: request.EmailAddress,
                OtpToken: request.OtpToken,
                MagicLinkToken: request.MagicLinkToken
            );

        Result<VerifyLoginResponse> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created((string?)null, result.Value)
            : CustomResults.Problem(result, httpContext);
    }
}
