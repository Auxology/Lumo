using FastEndpoints;

using Main.Application.Commands.Chats.Remix;

using Mediator;

using SharedKernel.Api.Constants;

namespace Main.Api.Endpoints.Chats.Remix;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/chats/{chatId}/remix");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Remix Chat")
                .WithDescription(
                    "Creates a copy of an existing chat with a different AI model and queues the last user message for re-generation.")
                .Produces<Response>(202, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(404, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        RemixChatCommand command = new
        (
            ChatId: endpointRequest.ChatId,
            NewModelId: endpointRequest.NewModelId,
            WebSearchEnabled: endpointRequest.WebSearchEnabled
        );

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: response => new Response
            (
                ChatId: response.ChatId,
                StreamId: response.StreamId,
                ChatTitle: response.ChatTitle,
                ModelId: response.ModelId,
                CreatedAt: response.CreatedAt
            ),
            successStatusCode: 202,
            cancellationToken: ct
        );
    }
}