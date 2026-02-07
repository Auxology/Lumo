using FastEndpoints;

namespace Main.Api.Endpoints.EphemeralChats.Stream;

internal sealed record Request
{
    [QueryParam]
    public required string StreamId { get; init; }
}