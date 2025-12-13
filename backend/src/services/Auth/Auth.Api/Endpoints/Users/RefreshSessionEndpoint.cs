using Auth.Application.Users.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Authentication;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class RefreshSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/non-passing/api/sessions", HandleAsync)
            .WithName("RefreshSession")
            .WithTags(Tags.Authentication)
            .WithDescription("Internal endpoint for refreshing access tokens using a valid refresh token")
            .ExcludeFromDescription()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "[Internal] Refresh access token using refresh token";
                operation.Description = """
                                        **INTERNAL USE ONLY** - Called exclusively by the Gateway API during token refresh flow.

                                        This endpoint handles access token renewal using a valid refresh token:
                                        - Validates the refresh token authenticity and expiration
                                        - Verifies user identity and account status
                                        - Generates new JWT access and refresh token pair
                                        - Invalidates the used refresh token (rotation)
                                        - Returns new tokens to Gateway for session continuation

                                        ## Service Responsibility
                                        The Auth service is responsible for:
                                        - Refresh token validation and verification
                                        - User status verification (not locked, not disabled)
                                        - New JWT token pair generation
                                        - Refresh token rotation (old token invalidation)
                                        - Token family tracking for reuse detection

                                        ## Gateway Integration
                                        The Gateway consumes this endpoint to:
                                        1. Forward client refresh requests (refresh token from HTTP-only cookie)
                                        2. Receive new token pair (access + refresh)
                                        3. Update cached access token server-side
                                        4. Set new refresh token as HTTP-only cookie for client
                                        5. Return session continuation response to client

                                        ## Request
                                        Provide the current valid refresh token in the request body.

                                        ## Response
                                        Returns `RefreshSessionResponse` containing:
                                        - `accessToken`: New short-lived JWT for API authorization
                                        - `refreshToken`: New long-lived token for next refresh cycle

                                        ## Security Features
                                        - Refresh token rotation (single-use tokens)
                                        - Token family tracking for reuse detection
                                        - Automatic session invalidation on token reuse (potential theft)
                                        - User status verification on each refresh
                                        - Token expiration validation

                                        ## Error Responses
                                        - `400 Bad Request`: Validation errors or malformed request
                                        - `401 Unauthorized`: Invalid, expired, or reused refresh token
                                        - `429 Too Many Requests`: Rate limit exceeded
                                        """;
                return Task.CompletedTask;
            })
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync
    (
        [FromBody] RefreshSessionRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        RefreshTokenCommand command = new(request.RefreshToken);

        Result<RefreshTokenResponse> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : CustomResults.Problem(result, httpContext);
    }
}
