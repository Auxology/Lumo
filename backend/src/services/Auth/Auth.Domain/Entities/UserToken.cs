using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using SharedKernel.Authentication;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Domain.Entities;

public sealed class UserToken : Entity<int>
{
    public UserId UserId { get; private set; }
    
    public string OtpTokenHash { get; private set; } = null!;
    
    public string MagicLinkTokenHash { get; private set; } = null!;
    
    public string IpAddress { get; private set; } = null!;
    
    public string UserAgent { get; private set; } = null!;
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset? UsedAt { get; private set; }
    
    private UserToken() { } // For EF Core

    [SetsRequiredMembers]
    private UserToken
    (
        UserId userId,
        string otpTokenHash,
        string magicLinkTokenHash,
        string ipAddress,
        string userAgent,
        DateTimeOffset utcNow
    )
    {
        Id = 0;
        UserId = userId;
        OtpTokenHash = otpTokenHash;
        MagicLinkTokenHash = magicLinkTokenHash;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(UserConstants.TokenExpirationMinutes);
        UsedAt = null;
    }
    
    internal bool IsUsed => UsedAt is not null;
    
    internal bool IsExpired(IDateTimeProvider dateTimeProvider)
    {
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        return utcNow >= ExpiresAt;
    }
    
    internal static Result<UserToken> Create
    (
        UserId userId,
        string otpTokenHash,
        string magicLinkTokenHash,
        string ipAddress,
        string userAgent,
        IDateTimeProvider dateTimeProvider
    )
    {
        if (userId.IsEmpty())
            return UserTokenErrors.UserIdRequired;
        
        if (string.IsNullOrWhiteSpace(otpTokenHash))
            return UserTokenErrors.OtpTokenHashRequired;
        
        if (string.IsNullOrWhiteSpace(magicLinkTokenHash))
            return UserTokenErrors.MagicLinkTokenHashRequired;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        UserToken userToken = new
        (
            userId: userId,
            otpTokenHash: otpTokenHash,
            magicLinkTokenHash: magicLinkTokenHash,
            ipAddress: ipAddress,
            userAgent: userAgent,
            utcNow: utcNow
        );
        
        return userToken;
    }
    
    private void MarkAsUsed(IDateTimeProvider dateTimeProvider)
    {
        UsedAt = dateTimeProvider.UtcNow;
    }

    internal Result Verify
    (
        string? otpToken,
        string? magicLinkToken,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider
    )
    {
        if (IsUsed)
            return UserTokenErrors.AlreadyUsed;

        if (IsExpired(dateTimeProvider))
            return UserTokenErrors.Expired;

        bool otpValid = !string.IsNullOrWhiteSpace(otpToken) && secretHasher.Verify(otpToken, OtpTokenHash);

        bool magicLinkValid = !string.IsNullOrWhiteSpace(magicLinkToken) && secretHasher.Verify(magicLinkToken, MagicLinkTokenHash);

        if (!otpValid && !magicLinkValid)
            return UserTokenErrors.InvalidToken;

        MarkAsUsed(dateTimeProvider);

        return Result.Success();
    }
}