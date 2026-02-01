using FastEndpoints;

using Main.Application.Commands.SharedChats.RefreshChat;

using Mediator;

namespace Main.Api.Endpoints.SharedChats.Refresh;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/shared-chats/{sharedChatId}/refresh");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Refresh Shared Chat")
                .WithDescription("Re-snapshots a shared chat with the latest messages from the source chat.")
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.SharedChats);
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        string sharedChatId = Route<string>("SharedChatId")!;

        RefreshSharedChatCommand command = new(sharedChatId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                SharedChatId: r.SharedChatId,
                SnapshotAt: r.SnapshotAt,
                CreatedAt: r.CreatedAt
            ),
            cancellationToken: ct
        );
    }
}