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
        app.MapPost("/api/sessions", HandleAsync)
            .WithName("CreateSession")
            .WithTags(Tags.Authentication)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Complete passwordless login";
                operation.Description = """
                                        Verifies the user's identity using either an OTP code or magic link token
                                        and issues JWT access and refresh tokens.

                                        **Authentication Options:**
                                        - Provide `otpToken` for 6-digit code verification
                                        - Provide `magicLinkToken` for magic link verification
                                        - Only one token type is required

                                        **Token Handling:**
                                        - Tokens are managed by the API gateway
                                        - Refresh token stored as HTTP-only cookie
                                        - Access token cached and paired with refresh token

                                        **Security:**
                                        - Failed attempts are logged
                                        - Account lockout after 5 failed attempts
                                        - Tokens are invalidated after successful verification
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

internal sealed record CreateSessionRequest
(
    string EmailAddress,
    string? OtpToken,
    string? MagicLinkToken
);