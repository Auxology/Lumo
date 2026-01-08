namespace Auth.Api.Endpoints.Sessions.GetActiveSessions;

internal sealed record SessionDto
(
    string Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? LastRefreshAt,
    string IpAddress,
    string Browser,
    string Os,
    bool IsCurrent
);

internal sealed record Response(IReadOnlyList<SessionDto> Sessions);