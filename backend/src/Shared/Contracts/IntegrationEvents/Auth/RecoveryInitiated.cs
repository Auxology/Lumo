namespace Contracts.IntegrationEvents.Auth;

public sealed record RecoveryInitiated
{
    public required Guid EventId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required Guid CorrelationId { get; init; }

    public required Guid UserId { get; init; }

    public required string OldEmailAddress { get; init; }

    public required string NewEmailAddress { get; init; }

    public required string OtpToken { get; init; }

    public required string MagicLinkToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string IpAddress { get; init; }

    public required string UserAgent { get; init; }
}