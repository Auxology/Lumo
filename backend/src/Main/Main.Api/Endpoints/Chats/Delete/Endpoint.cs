using FastEndpoints;

using Main.Application.Chats.Delete;

using Mediator;

namespace Main.Api.Endpoints.Chats.Delete;

internal sealed class Endpoint : BaseEndpoint<Request>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }
    
    public override void Configure()
    {
        Delete("/api/chats/{chatId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Delete Chat")
                .WithDescription("Permanently deletes a chat and all its messages.")
                .Produces(204)
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(403, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        DeleteChatCommand command = new(endpointRequest.ChatId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct
        );
    }
}