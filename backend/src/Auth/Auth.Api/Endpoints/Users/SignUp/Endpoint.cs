using Auth.Application.Users.SignUp;
using FastEndpoints;
using Mediator;

namespace Auth.Api.Endpoints.Users.SignUp;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
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
                .Produces<Response>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(409, "application/json")
                .WithTags(CustomTags.Users);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        SignUpCommand command = new
        (
            DisplayName: endpointRequest.DisplayName,
            EmailAddress: endpointRequest.EmailAddress
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response(UserFriendlyRecoveryKeys: r.UserFriendlyRecoveryKeys),
            successStatusCode: 201,
            ct
        );
    }
}
