using Auth.Application.LoginRequests.Create;
using FastEndpoints;
using Mediator;

namespace Auth.Api.Endpoints.LoginRequests.Create;

internal sealed class
    Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }
    
    public override void Configure()
    {
        Post("/api/login-requests");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Sign Up")
                .WithDescription("Creates a new user account.")
                .Produces<Response>(201, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .WithTags(CustomTags.LoginRequests);
        });
    }
    
    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        CreateLoginCommand command = new(endpointRequest.EmailAddress);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: clrr => new Response
                (
                    TokenKey: clrr.TokenKey,
                    ExpiresAt: clrr.ExpiresAt
                ),
            successStatusCode: 201,
            ct
        );
    }
}