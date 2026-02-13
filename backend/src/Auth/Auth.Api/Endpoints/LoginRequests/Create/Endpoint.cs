using Auth.Application.Commands.LoginRequests.Create;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

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
            d.WithSummary("Create Login Request")
                .WithDescription("Initiates a login request and sends OTP/magic link to the user's email.")
                .Produces<Response>(201, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.LoginRequests);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        CreateLoginCommand command = new(endpointRequest.EmailAddress);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: clr => new Response
                (
                    TokenKey: clr.TokenKey,
                    ExpiresAt: clr.ExpiresAt
                ),
            successStatusCode: 201,
            ct
        );
    }
}