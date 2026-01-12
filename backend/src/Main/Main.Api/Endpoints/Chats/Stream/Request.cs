using FastEndpoints;

namespace Main.Api.Endpoints.Chats.Stream;

internal sealed record Request
{
    [RouteParam]
    public required string ChatId { get; init; }

    [QueryParam]
    public required string StreamId { get; init; }
}