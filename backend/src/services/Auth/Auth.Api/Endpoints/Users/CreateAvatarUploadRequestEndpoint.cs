using Auth.Application.Users.RequestAvatarUpload;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SharedKernel.Api.Endpoints;
using SharedKernel.Api.Infrastructure;
using SharedKernel.ResultPattern;

namespace Auth.Api.Endpoints.Users;

internal sealed class CreateAvatarUploadRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/me/avatar-upload-requests", HandleAsync)
            .WithName("CreateAvatarUploadRequest")
            .WithTags(Tags.Users, Tags.Avatars)
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Request presigned URL for avatar upload";
                operation.Description = """
                                        Initiates the avatar upload process by generating a presigned S3 URL
                                        for direct browser-to-S3 upload.

                                        **Upload Flow:**
                                        1. Call this endpoint to get presigned URL
                                        2. Upload file directly to S3 using the presigned URL
                                        3. Call confirm endpoint to finalize the upload

                                        **File Requirements:**
                                        - Supported formats: JPEG, PNG, WebP
                                        - Maximum file size: 5MB
                                        - Recommended dimensions: 512x512 pixels (will be resized)

                                        **Presigned URL:**
                                        - Valid for 15 minutes
                                        - Single-use only
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
            .Produces<RequestAvatarUploadResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
    
    private static async Task<IResult> HandleAsync
    (
        [FromBody] CreateAvatarUploadRequestRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        RequestAvatarUploadCommand command = new
        (
            ContentType: request.ContentType,
            ContentLength: request.FileSizeBytes
        );
        
        Result<RequestAvatarUploadResponse> result = await sender.Send(command, cancellationToken);
        
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : CustomResults.Problem(result, httpContext);
    }
}

internal sealed record CreateAvatarUploadRequestRequest
(
    string ContentType,
    long FileSizeBytes
);