using Auth.Application.Commands.Users.CancelDeletion;

using FastEndpoints;

using Mediator;

using SharedKernel.Api.Constants;

namespace Auth.Api.Endpoints.Users.CancelDeletion;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender) => _sender = sender;

    public override void Configure()
    {
        Delete("/api/users/me/deletion");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Cancel User Deletion")
                .WithDescription("Cancels the deletion process of the authenticated user's account.")
                .Produces<Response>(200, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(401)
                .ProducesProblemDetails(404)
                .ProducesProblemDetails(409)
                .WithTags(CustomTags.Users);
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        CancelUserDeletionCommand command = new();

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: cudr => new Response
            (
                Id: cudr.Id,
                CanceledAt: cudr.CanceledAt
            ),
            cancellationToken: ct
        );
    }
}