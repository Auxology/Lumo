using FastEndpoints;

using Main.Application.Commands.SharedChats.ForkSharedChat;

using Mediator;

using SharedKernel.Api.Constants;

namespace Main.Api.Endpoints.SharedChats.Fork;

internal sealed class Endpoint : BaseEndpoint<EmptyRequest, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/api/shared-chats/{sharedChatId}/fork");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Fork Shared Chat")
                .WithDescription("Creates a new chat by copying all messages from a shared chat snapshot.")
                .Produces<Response>(201, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(404, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.SharedChats);
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        string sharedChatId = Route<string>("SharedChatId")!;

        ForkSharedChatCommand command = new(sharedChatId);

        await SendOutcomeAsync
        (
            outcome: await _sender.Send(command, ct),
            mapper: r => new Response
            (
                ChatId: r.ChatId,
                Title: r.Title,
                ModelId: r.ModelId,
                CreatedAt: r.CreatedAt
            ),
            successStatusCode: 201,
            ct
        );
    }
}