using Auth.Application.Users.ChangeName;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class ChangeNameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/me/name", HandleAsync)
            .WithName("ChangeUserName")
            .WithTags(Tags.Users)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Update user display name";
                operation.Description = """
                                        Updates the authenticated user's display name.

                                        **Authentication Required:** Bearer token must be provided in Authorization header.

                                        **Business Rules:**
                                        - Display name must be between 2 and 50 characters
                                        - Display name cannot be empty or whitespace only
                                        """;
                operation.Security = 
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer")] = []
                    }
                ];
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
    
    private static async Task<IResult> HandleAsync
    (
        [FromBody] ChangeNameRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        ChangeNameCommand command = new
        (
            NewDisplayName: request.NewDisplayName
        );
        
        Result result = await sender.Send(command, cancellationToken);
        
        return result.IsSuccess
            ? Results.NoContent()
            : CustomResults.Problem(result, httpContext);
    }
}

internal sealed record ChangeNameRequest
(
    string NewDisplayName
);