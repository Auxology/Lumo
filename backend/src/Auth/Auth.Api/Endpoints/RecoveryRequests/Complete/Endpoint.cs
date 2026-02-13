using Auth.Application.Commands.RecoveryRequests.Complete;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

namespace Auth.Api.Endpoints.RecoveryRequests.Complete;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/recovery-requests/{TokenKey}/complete");
        AllowAnonymous();
        Version(1);

        Description(d =>
        {
            d.WithSummary("Complete Account Recovery")
                .WithDescription("Completes recovery: changes email, revokes sessions.")
                .Produces<Response>(200, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Recovery);
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        string tokenKey = Route<string>("TokenKey")!;

        CompleteRecoveryCommand command = new(TokenKey: tokenKey);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response(NewEmailAddress: r.NewEmailAddress),
            successStatusCode: 200,
            ct
        );
    }
}