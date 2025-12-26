namespace Contracts.IntegrationEvents.Auth;

public sealed record UserSignedUp
{
    public required Guid EventId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required Guid CorrelationId { get; init; }

    public required string DisplayName { get; init; }

    public required string EmailAddress { get; init; }
};
