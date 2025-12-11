using Auth.Application.Users.FinalizeAvatarUpload;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class UpdateAvatarEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/me/avatar", HandleAsync)
            .WithName("UpdateAvatar")
            .WithTags(Tags.Users, Tags.Avatars)
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Finalize avatar upload";
                operation.Description = """
                                        Completes the avatar upload process after the file has been uploaded to S3.

                                        **Processing:**
                                        - Validates the uploaded file exists in S3
                                        - Generates thumbnail versions
                                        - Updates user profile with new avatar URL
                                        - Deletes previous avatar if exists

                                        **Important:**
                                        - Must be called within 1 hour of receiving the presigned URL
                                        - Avatar key must match the one from CreateAvatarUploadRequest
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync
    (
        [FromBody] UpdateAvatarRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var command = new FinalizeAvatarUploadCommand
        (
            AvatarKey: request.AvatarKey
        );

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : CustomResults.Problem(result, httpContext);
    }
}

internal sealed record UpdateAvatarRequest(string AvatarKey);