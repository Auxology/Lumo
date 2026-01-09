using FastEndpoints;

using Main.Application.Commands.Chats.Update;

using Mediator;

namespace Main.Api.Endpoints.Chats.Update;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/chats/{chatId}");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Update Chat")
                .WithDescription("Partially updates a chat. Supports renaming and archiving/unarchiving.")
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        UpdateChatCommand command = new
        (
            ChatId: endpointRequest.ChatId,
            NewTitle: endpointRequest.NewTitle,
            IsArchived: endpointRequest.IsArchived
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            cancellationToken: ct,
            mapper: ucr => new Response
            (
                ChatId: ucr.ChatId,
                Title: ucr.Title,
                IsArchived: ucr.IsArchived,
                UpdatedAt: ucr.UpdatedAt
            )
        );
    }
}