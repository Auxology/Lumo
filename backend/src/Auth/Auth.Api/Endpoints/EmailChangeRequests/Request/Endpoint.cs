using Auth.Application.Commands.EmailChangeRequests.Request;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

namespace Auth.Api.Endpoints.EmailChangeRequests.Request;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/email-change-requests");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Request Email Change")
                .WithDescription("Initiates an email change request. Sends verification to the new email address.")
                .Produces<Response>(201, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(409, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.EmailChangeRequests);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        RequestEmailChangeCommand command = new
        (
            NewEmailAddress: endpointRequest.NewEmailAddress
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: recr => new Response(RequestId: recr.RequestId),
            successStatusCode: 201,
            ct
        );
    }
}