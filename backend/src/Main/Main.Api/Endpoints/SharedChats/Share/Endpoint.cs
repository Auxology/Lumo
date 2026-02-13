using FastEndpoints;

using Main.Application.Commands.SharedChats.ShareChat;

using Mediator;

using SharedKernel.Api.Constants;

namespace Main.Api.Endpoints.SharedChats.Share;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/shared-chats");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Share Chat")
                .WithDescription("Creates a public snapshot of a chat that can be viewed without authentication.")
                .Produces<Response>(201, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(404, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.SharedChats);
        });
    }

    public override async Task HandleAsync(Request endpointRequest, CancellationToken ct)
    {
        ShareChatCommand command = new(endpointRequest.ChatId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: scr => new Response
            (
                SharedChatId: scr.SharedChatId,
                SourceChatId: scr.SourceChatId,
                Title: scr.Title,
                ModelId: scr.ModelId,
                SnapshotAt: scr.SnapshotAt,
                CreatedAt: scr.CreatedAt
            ),
            successStatusCode: 201,
            ct
        );
    }
}