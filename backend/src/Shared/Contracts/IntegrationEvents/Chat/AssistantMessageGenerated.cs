namespace Contracts.IntegrationEvents.Chat;

public sealed record AssistantMessageGenerated
{
    public required Guid EventId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required Guid CorrelationId { get; init; }

    public required Guid ChatId { get; init; }

    public required string MessageContent { get; init; }
}