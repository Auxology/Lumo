using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Enums;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;
using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class Session : AggregateRoot<SessionId>
{
    public UserId UserId { get; private set; }

    public string RefreshTokenKey { get; private set; } = string.Empty;
    
    public string RefreshTokenHash { get; private set; } = string.Empty;
    
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

    public static Outcome<Session> Create
    (
        UserId userId,
        string refreshTokenKey,
        string refreshTokenHash,
        Fingerprint fingerprint,
        DateTimeOffset utcNow
    )
    {
        if (userId.IsEmpty)
            return SessionFaults.UserIdRequiredForCreation;
        
        if (string.IsNullOrWhiteSpace(refreshTokenKey))
            return SessionFaults.RefreshTokenKeyRequiredForCreation;
        
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            return SessionFaults.RefreshTokenHashRequiredForCreation;

        Session session = new
        (
            userId: userId,
            refreshTokenKey: refreshTokenKey,
            refreshTokenHash: refreshTokenHash,
            fingerprint: fingerprint,
            utcNow: utcNow
        );
        
        return session;
    }

    public Outcome Refresh
    (
        string newRefreshTokenKey,
        string newRefreshTokenHash,
        DateTimeOffset utcNow
    )
    {
        if (string.IsNullOrWhiteSpace(newRefreshTokenKey))
            return SessionFaults.RefreshTokenKeyRequiredForRefresh;
        
        if (string.IsNullOrWhiteSpace(newRefreshTokenHash))
            return SessionFaults.RefreshTokenHashRequiredForRefresh;
        
        Outcome ensureActiveOutcome = EnsureActive(utcNow);
        
        if (ensureActiveOutcome.IsFailure)
            return ensureActiveOutcome;
        
        RefreshTokenKey = newRefreshTokenKey;
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = utcNow.AddDays(SessionConstants.SessionExpirationDays);
        LastRefreshedAt = utcNow;
        Version += 1;
        
        return Outcome.Success();
    }

    public Outcome RevokeDueToLogout(DateTimeOffset utcNow)
    {
        Outcome ensureActiveOutcome = EnsureActive(utcNow);
        
        if (ensureActiveOutcome.IsFailure)
            return ensureActiveOutcome;
        
        RevokeReason = SessionRevokeReason.UserLogout;
        RevokedAt = utcNow;
        
        return Outcome.Success();
    }
    
    public Outcome RevokeDueToEmailChange(DateTimeOffset utcNow)
    {
        Outcome ensureActiveOutcome = EnsureActive(utcNow); 
        
        if (ensureActiveOutcome.IsFailure)
            return ensureActiveOutcome;
        
        RevokeReason = SessionRevokeReason.EmailChange;
        RevokedAt = utcNow;
        
        return Outcome.Success();
    }

    private Outcome EnsureActive(DateTimeOffset utcNow)
    {
        if (ExpiresAt <= utcNow)
            return SessionFaults.SessionExpired;
        
        if (RevokedAt is not null)
            return SessionFaults.SessionRevoked;
        
        return Outcome.Success();
    }
}