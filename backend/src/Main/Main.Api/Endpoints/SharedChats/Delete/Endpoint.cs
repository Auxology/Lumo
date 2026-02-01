using FastEndpoints;

using Main.Application.Commands.SharedChats.DeleteSharedChat;

using Mediator;

namespace Main.Api.Endpoints.SharedChats.Delete;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/api/shared-chats/{sharedChatId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Delete Shared Chat")
                .WithDescription("Permanently deletes a shared chat snapshot.")
                .Produces(204)
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.SharedChats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        DeleteSharedChatCommand command = new(endpointRequest.SharedChatId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}