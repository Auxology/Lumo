using FastEndpoints;

using Main.Application.Chats.Starts;

using Mediator;

namespace Main.Api.Endpoints.Chats.Start;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/chats");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Start Chat")
                .WithDescription("Creates a new chat and queues the initial message for AI processing.")
                .Produces<Response>(202, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        StartChatCommand command = new(endpointRequest.Message);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                ChatId: r.ChatId,
                ChatTitle: r.ChatTitle,
                CreatedAt: r.CreatedAt
            ),
            successStatusCode: 202,
            ct
        );
    }
}