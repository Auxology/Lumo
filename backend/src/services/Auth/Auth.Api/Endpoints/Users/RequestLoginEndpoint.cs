using Auth.Application.Users.RequestLogin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class RequestLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/login-requests", HandleAsync)
            .WithName("RequestLogin")
            .WithTags(Tags.Authentication)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Initiate passwordless login";
                operation.Description = """
                                        Starts the passwordless authentication flow by sending a magic link
                                        and OTP code to the user's email address.

                                        **Authentication Flow:**
                                        1. User submits email address
                                        2. System generates magic link and OTP
                                        3. Email is sent with both authentication options
                                        4. User completes login using VerifyLogin endpoint

                                        **Security:**
                                        - OTP expires after 10 minutes
                                        - Magic link expires after 15 minutes
                                        - Rate limiting applied (5 requests per 15 minutes per email)
                                        """;
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }

    private static async Task<IResult> HandleAsync
    (
        [FromBody] RequestLoginRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        RequestLoginCommand command = new
        (
            EmailAddress: request.EmailAddress
        );
        
        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Accepted(value: new { message = "Login request processed. Check your email." })
            : CustomResults.Problem(result, httpContext);
    }
}

internal sealed record RequestLoginRequest
(
    string EmailAddress  
);