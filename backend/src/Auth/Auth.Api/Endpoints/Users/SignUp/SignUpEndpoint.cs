using Auth.Application.Users.SignUp;
using FastEndpoints;
using Mediator;

namespace Auth.Api.Endpoints.Users.SignUp;

internal sealed class SignUpEndpoint : BaseEndpoint<SignUpRequest, SignUpEndpointResponse>
{
    private readonly ISender _sender;

    public SignUpEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Sign Up")
                .WithDescription("Creates a new user account.")
                .Produces<SignUpEndpointResponse>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(409, "application/json")
                .WithTags(CustomTags.Users);
        });
    }

    public override async Task HandleAsync(SignUpRequest request, CancellationToken ct)
    {
        SignUpCommand command = new
        (
            DisplayName: request.DisplayName,
            EmailAddress: request.EmailAddress
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new SignUpEndpointResponse(UserFriendlyRecoveryKeys: r.UserFriendlyRecoveryKeys),
            successStatusCode: 201,
            ct
        );
    }

}
