using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class Session : AggregateRoot<SessionId>
{
    public UserId UserId { get; private set; }
    
    public string RefreshTokenKey { get; private set; }
    
    public string RefreshTokenHash { get; private set; }
    
    public Fingerprint Fingerprint { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset? LastRefreshedAt { get; private set; }
    
    public SessionRevokeReason? RevokeReason { get; private set; }
    
    public DateTimeOffset? RevokedAt { get; private set; }
    
    public int Version { get; private set; }
    
    private Session() {} // For EF Core

    [SetsRequiredMembers]
    private Session
    (
        UserId userId,
        string refreshTokenKey,
        string refreshTokenHash,
        Fingerprint fingerprint,
        DateTimeOffset utcNow
    )
    {
        Id = SessionId.New();
        UserId = userId;
        RefreshTokenKey = refreshTokenKey;
        RefreshTokenHash = refreshTokenHash;
        Fingerprint = fingerprint;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddDays(SessionConstants.SessionExpirationDays);
        LastRefreshedAt = utcNow;
        RevokeReason = null;
        RevokedAt = null;
        Version = 1;
    }
}