using Auth.Application.Commands.RecoveryRequests.Complete;

using FastEndpoints;

using Mediator;

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
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(401, "application/json")
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