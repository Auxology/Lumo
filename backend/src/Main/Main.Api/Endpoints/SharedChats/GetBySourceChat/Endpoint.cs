using FastEndpoints;

using Main.Application.Queries.SharedChats.GetSharedChatsByOriginal;

using Mediator;

using SharedKernel.Api.Constants;

namespace Main.Api.Endpoints.SharedChats.GetBySourceChat;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/chats/{chatId}/shared-chats");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Shared Chats by Source Chat")
                .WithDescription(
                    "Retrieves all shared chat snapshots created from a specific source chat. " +
                    "Only returns shared chats owned by the authenticated user.")
                .Produces<Response>(200, HttpContentTypeConstants.Json)
                .ProducesProblemDetails(400, HttpContentTypeConstants.Json)
                .WithTags(CustomTags.SharedChats);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        GetSharedChatByOriginalQuery query = new(request.ChatId);

        await SendOutcomeAsync(
            outcome: await _sender.Send(query, ct),
            mapper: response => new Response(
                SharedChats: response.SharedChats
                    .Select(sc => new SharedChatDto(
                        Id: sc.Id,
                        SourceChatId: sc.SourceChatId,
                        OwnerId: sc.OwnerId,
                        Title: sc.Title,
                        ModelId: sc.ModelId,
                        ViewCount: sc.ViewCount,
                        SnapshotAt: sc.SnapshotAt,
                        CreatedAt: sc.CreatedAt))
                    .ToList()),
            cancellationToken: ct);
    }
}