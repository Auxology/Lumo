using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using SharedKernel.Constants;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Domain.Aggregates.Session;

public sealed class Session : AggregateRoot<SessionId>
{
    public UserId UserId { get; private set; }

    public string HashedRefreshToken { get; private set; } = null!;

    public string IpAddress { get; private set; } = null!;

    public string UserAgent { get; private set; } = null!;
    
    public int Version { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset? RevokedAt { get; private set; }
    
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
        RevokedAt = null;
    }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        return dateTimeProvider.UtcNow >= ExpiresAt;
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

        if (string.IsNullOrWhiteSpace(ipAddress))
            ipAddress = RequestConstants.IpAddressFallback;
        
        if (string.IsNullOrWhiteSpace(userAgent))
            userAgent = RequestConstants.UserAgentFallback;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        Session session = new
        (
            userId: userId,
            hashedRefreshToken: hashedRefreshToken,
            ipAddress: ipAddress,
            userAgent: userAgent,
            utcNow: utcNow
        );
        
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
        
        return Result.Success();
    }

    public Result Refresh(string newHashedRefreshToken, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        if (string.IsNullOrWhiteSpace(newHashedRefreshToken))
            return SessionErrors.HashedRefreshTokenRequired;
        
        if (!IsValid(dateTimeProvider))
            return SessionErrors.Invalid;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        HashedRefreshToken = newHashedRefreshToken;
        ExpiresAt = utcNow.AddDays(SessionConstants.ExpirationDays);
        Version += 1;
        
        return Result.Success();
    }
}