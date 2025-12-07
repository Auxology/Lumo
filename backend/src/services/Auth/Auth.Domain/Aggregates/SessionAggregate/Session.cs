using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.Events.SessionEvents;
using Auth.Domain.ValueObjects;
using SharedKernel.Constants;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Domain.Aggregates.SessionAggregate;

public sealed class Session : AggregateRoot<SessionId>
{
    public UserId UserId { get; private set; }

    public string HashedRefreshToken { get; private set; } = null!;

    public string IpAddress { get; private set; } = null!;

    public string UserAgent { get; private set; } = null!;
    
    public int Version { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset AbsoluteExpiresAt { get; private set; }
    
    public DateTimeOffset? RevokedAt { get; private set; }
    
    public DateTimeOffset? LastRefreshedAt { get; private set; }
    
    private Session() { } // For EF Core
    
    [SetsRequiredMembers]
    private Session
    (
        UserId userId,
        string hashedRefreshToken,
        string ipAddress,
        string userAgent,
        DateTimeOffset utcNow
    )
    {
        Id = SessionId.New();
        UserId = userId;
        HashedRefreshToken = hashedRefreshToken;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Version = 1;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddDays(SessionConstants.ExpirationDays);
        AbsoluteExpiresAt = utcNow.AddDays(SessionConstants.AbsoluteExpirationDays);
        RevokedAt = null;
        LastRefreshedAt = null;
    }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        return utcNow >= ExpiresAt || utcNow >= AbsoluteExpiresAt;
    }

    public bool IsValid(IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        return !IsRevoked && !IsExpired(dateTimeProvider);
    }

    public static Result<Session> Create
    (
        UserId userId,
        string hashedRefreshToken,
        string? ipAddress,
        string? userAgent,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        if (userId.IsEmpty())
            return SessionErrors.UserIdRequired;

        if (string.IsNullOrWhiteSpace(hashedRefreshToken))
            return SessionErrors.HashedRefreshTokenRequired;

        string finalIpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? RequestConstants.IpAddressFallback
            : ipAddress;
        
        string finalUserAgent = string.IsNullOrWhiteSpace(userAgent)
            ? RequestConstants.UserAgentFallback
            : userAgent;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        Session session = new
        (
            userId: userId,
            hashedRefreshToken: hashedRefreshToken,
            ipAddress: finalIpAddress,
            userAgent: finalUserAgent,
            utcNow: utcNow
        );

        SessionCreatedDomainEvent domainEvent = new
        (
            SessionId: session.Id.Value,
            UserId: session.UserId.Value,
            IpAddress: session.IpAddress,
            UserAgent: session.UserAgent,
            OccurredOn: utcNow
        );
        
        session.AddDomainEvent(domainEvent);
        
        return session;
    }

    public Result Revoke(IDateTimeProvider dateTimeProvider)
    {
        if (IsRevoked)
            return SessionErrors.AlreadyRevoked;

        if (IsExpired(dateTimeProvider))
            return Result.Success();
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        RevokedAt = utcNow;

        SessionRevokedDomainEvent domainEvent = new
        (
            SessionId: Id.Value,
            UserId: UserId.Value,
            Reason: SessionConstants.RevokedByUser,
            OccurredOn: utcNow
        );
        
        AddDomainEvent(domainEvent);
        
        return Result.Success();
    }

    public Result Refresh
    (
        string newHashedRefreshToken,
        string? ipAddress,
        string? userAgent,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        if (string.IsNullOrWhiteSpace(newHashedRefreshToken))
            return SessionErrors.HashedRefreshTokenRequired;

        if (!IsValid(dateTimeProvider))
            return SessionErrors.Invalid;

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        HashedRefreshToken = newHashedRefreshToken;
        ExpiresAt = utcNow.AddDays(SessionConstants.ExpirationDays);
        LastRefreshedAt = utcNow;
        Version += 1;

        SessionRefreshedDomainEvent domainEvent = new
        (
            SessionId: Id.Value,
            UserId: UserId.Value,
            NewVersion: Version,
            OccurredOn: utcNow,
            NewExpiresAt: ExpiresAt
        );

        AddDomainEvent(domainEvent);

        return Result.Success();
    }
}