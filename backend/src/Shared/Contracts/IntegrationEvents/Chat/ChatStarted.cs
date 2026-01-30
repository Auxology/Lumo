namespace Contracts.IntegrationEvents.Chat;

public sealed record ChatStarted
{
    public required Guid EventId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required Guid CorrelationId { get; init; }

    public required string ChatId { get; init; }

    public required string StreamId { get; init; }

    public required Guid UserId { get; init; }

    public required string ModelId { get; init; }

    public required string InitialMessage { get; init; }
}