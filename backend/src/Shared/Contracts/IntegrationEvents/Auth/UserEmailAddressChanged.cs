namespace Contracts.IntegrationEvents.Auth;

public sealed record UserEmailAddressChanged
{
    public required Guid EventId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required Guid CorrelationId { get; init; }

    public required Guid UserId { get; init; }

    public required string OldEmailAddress { get; init; }

    public required string NewEmailAddress { get; init; }
};