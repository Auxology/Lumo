namespace Main.Application.Abstractions.Stream;

public sealed record StreamMessage(
    StreamMessageType Type,
    string Content,
    DateTimeOffset Timestamp
)
{
    public string? Query { get; init; }

    public string? Sources { get; init; }
}