using FastEndpoints;

using Main.Application.Commands.EphemeralChats.SendEphemeralMessage;

using Mediator;

namespace Main.Api.Endpoints.EphemeralChats.SendEphemeralMessage;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/ephemeral-chats/{ephemeralChatId}/messages");
        Version(1);

        Options(o => o.RequireRateLimiting("ai-generation"));

        Description(d =>
        {
            d.WithSummary("Send Ephemeral Message")
                .WithDescription(
                    "Sends a user message to an existing ephemeral chat and queues AI response generation. " +
                    "The AI response will be streamed via Redis Pub/Sub.")
                .Produces<Response>(202, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.EphemeralChats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        SendEphemeralMessageCommand command = new
        (
            EphemeralChatId: endpointRequest.EphemeralChatId,
            Message: endpointRequest.Message
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: response => new Response
            (
                EphemeralChatId: response.EphemeralChatId,
                StreamId: response.StreamId,
                MessageRole: response.MessageRole,
                MessageContent: response.MessageContent,
                CreatedAt: response.CreatedAt
            ),
            successStatusCode: 202,
            cancellationToken: ct
        );
    }
}