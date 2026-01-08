namespace Auth.Application.Queries.Sessions.GetActiveSessions;

public sealed record ActiveSessionReadModel
{
    public required string Id { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset? LastRefreshAt { get; init; }

    public required string IpAddress { get; init; }

    public required string NormalizedBrowser { get; init; }

    public required string NormalizedOs { get; init; }

    public bool IsCurrent { get; init; }
}