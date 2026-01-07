using FastEndpoints;

using Main.Application.Chats.SendMessage;

using Mediator;

namespace Main.Api.Endpoints.Chats.SendMessage;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/chats/{chatId}/messages");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Send Message")
                .WithDescription(
                    "Sends a user message to an existing chat and queues AI response generation. " +
                    "The AI response will be streamed via Redis Pub/Sub.")
                .Produces<Response>(202, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        SendMessageCommand command = new
        (
            ChatId: endpointRequest.ChatId,
            Message: endpointRequest.Message
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: response => new Response
            (
                MessageId: response.MessageId,
                ChatId: response.ChatId,
                MessageRole: response.MessageRole,
                MessageContent: response.MessageContent,
                CreatedAt: response.CreatedAt
            ),
            successStatusCode: 202,
            cancellationToken: ct
        );
    }
}