using Auth.Application.Users.UpdateProfile;
using FastEndpoints;
using Mediator;

namespace Auth.Api.Endpoints.Users.UpdateProfile;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/users/me");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Update Profile")
                .WithDescription("Updates the current user's profile. All fields are optional.")
                .Produces(204)
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Users);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        UpdateProfileCommand command = new
        (
            NewDisplayName: endpointRequest.NewDisplayName,
            NewAvatarKey: endpointRequest.NewAvatarKey
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}
