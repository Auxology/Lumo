using Auth.Application.Commands.Users.GetAvatarUploadUrl;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.Users.GetAvatarUploadUrl;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/users/me/avatar");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Avatar Upload URL")
                .WithDescription("Generates a presigned URL for uploading a user avatar to S3.")
                .Produces<Response>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Users);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        GetAvatarUploadUrlCommand command = new
        (
            ContentType: endpointRequest.ContentType,
            ContentLength: endpointRequest.ContentLength
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                UploadUrl: r.UploadUrl,
                AvatarKey: r.AvatarKey,
                ExpiresAt: r.ExpiresAt
            ),
            successStatusCode: 201,
            cancellationToken: ct
        );
    }
}