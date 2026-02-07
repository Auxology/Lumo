using FastEndpoints;

using Main.Application.Queries.Chats.GetMessages;

using Mediator;

namespace Main.Api.Endpoints.Chats.GetMessages;

internal sealed class Endpoint : BaseEndpoint<Request, Response>
{
    private readonly ISender _sender;

    public Endpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/api/chats/{chatId}/messages");
        Version(1);

        Description(d =>
        {
            d.WithSummary("Get Messages")
                .WithDescription(
                    "Retrieves paginated messages for a chat. " +
                    "Messages are returned in chronological order (oldest first). " +
                    "Use the cursor parameter to load older messages.")
                .Produces<Response>(200, "application/json")
                .ProducesProblemDetails(400, "application/json")
                .ProducesProblemDetails(404, "application/json")
                .WithTags(CustomTags.Chats);
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        GetMessagesQuery query = new
        (
            ChatId: request.ChatId,
            Cursor: request.Cursor,
            Limit: request.Limit
        );

        await SendOutcomeAsync(
            outcome: await _sender.Send(query, ct),
            mapper: response => new Response(
                Messages: response.Messages
                    .Select(m => new MessageDto
                    (
                        Id: m.Id,
                        ChatId: m.ChatId,
                        MessageContent: m.MessageContent,
                        MessageRole: m.MessageRole,
                        TokenCount: m.TokenCount,
                        SequenceNumber: m.SequenceNumber,
                        CreatedAt: m.CreatedAt,
                        EditedAt: m.EditedAt
                    ))
                    .ToList(),
                Pagination: new PaginationDto
                (
                    NextCursor: response.Pagination.NextCursor,
                    HasMore: response.Pagination.HasMore,
                    Limit: response.Pagination.Limit
                )
            ),
            successStatusCode: 200,
            cancellationToken: ct
        );
    }
}