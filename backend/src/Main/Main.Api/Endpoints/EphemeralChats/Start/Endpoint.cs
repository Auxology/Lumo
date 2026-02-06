using FastEndpoints;

using Main.Application.Commands.EphemeralChats.Start;

using Mediator;

namespace Main.Api.Endpoints.EphemeralChats.Start;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/ephemeral-chats");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Start Ephemeral Chat")
                .WithDescription(
                    "Creates a new ephemeral chat session stored in Redis with automatic expiration. " +
                    "No chat history is persisted to the database.")
                .Produces<Response>(202, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.EphemeralChats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        StartEphemeralChatCommand command = new
        (
            Message: endpointRequest.Message,
            ModelId: endpointRequest.ModelId
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                EphemeralChatId: r.EphemeralChatId,
                StreamId: r.StreamId
            ),
            successStatusCode: 202,
            ct
        );
    }
}