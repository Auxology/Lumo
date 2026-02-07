using FastEndpoints;

using Main.Application.Commands.Chats.EditMessage;

using Mediator;

namespace Main.Api.Endpoints.Chats.EditMessage;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Patch("/api/chats/{chatId}/messages/{messageId}");
        Version(1);

        Options(o => o.RequireRateLimiting("ai-generation"));

        Description(d =>
        {
            d.WithSummary("Edit Message")
                .WithDescription(
                    "Edits an existing user message and removes all subsequent messages. " +
                    "Queues AI response generation for the edited message. " +
                    "The AI response will be streamed via Redis Pub/Sub.")
                .Produces<Response>(202, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        EditMessageCommand command = new
        (
            ChatId: endpointRequest.ChatId,
            MessageId: endpointRequest.MessageId,
            NewContent: endpointRequest.NewContent
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: response => new Response
            (
                ChatId: response.ChatId,
                MessageId: response.MessageId,
                StreamId: response.StreamId,
                MessageRole: response.MessageRole,
                MessageContent: response.MessageContent,
                EditedAt: response.EditedAt
            ),
            successStatusCode: 202,
            cancellationToken: ct
        );
    }
}