using Auth.Application.Commands.Users.RequestDeletion;

using FastEndpoints;

using Mediator;

namespace Auth.Api.Endpoints.Users.RequestDeletion;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Delete("/api/users/me");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Request Account Deletion")
                .WithDescription("Initiates the process to delete the currently authenticated user's account.")
                .Produces<Response>(200)
                .ProducesProblemDetails(401)
                .ProducesProblemDetails(404)
                .WithTags(CustomTags.Users);
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        RequestUserDeletionCommand command = new();

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: rudr => new Response
            (
                Id: rudr.Id,
                RequestedAt: rudr.RequestedAt,
                WillBeDeletedAt: rudr.WillBeDeletedAt
            ),
            cancellationToken: ct
        );
    }
}