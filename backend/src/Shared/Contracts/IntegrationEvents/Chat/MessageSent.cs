namespace Contracts.IntegrationEvents.Chat;

public sealed record MessageSent
{
    public required Guid EventId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required Guid CorrelationId { get; init; }

    public required string ChatId { get; init; }

    public required Guid UserId { get; init; }

    public required string Message { get; init; }
};