using Auth.Application.Users.SignUp;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class SignUpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sign-ups", HandleAsync)
            .WithName("SignUp")
            .WithTags(Tags.Users, Tags.Authentication)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Register a new user account";
                operation.Description = """
                                        Creates a new user account with email-based authentication.

                                        **Important:** The response includes recovery codes that should be securely
                                        stored by the user. These codes are shown only once and can be used for
                                        account recovery if access is lost.

                                        **Business Rules:**
                                        - Email address must be unique
                                        - Display name is required
                                        - Recovery codes are automatically generated (8 codes, 10 characters each)
                                        """;
                return Task.CompletedTask;
            })
            .Produces<SignUpResponse>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync
    (
        [FromBody] SignUpRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        SignUpCommand command = new
        (
            DisplayName: request.DisplayName,
            EmailAddress: request.EmailAddress
        );
        
        Result<SignUpResponse> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/users/{result.Value.UserId}", result.Value)
            : CustomResults.Problem(result, httpContext);
    }
}

internal sealed record SignUpRequest
(
    string DisplayName,
    string EmailAddress
);