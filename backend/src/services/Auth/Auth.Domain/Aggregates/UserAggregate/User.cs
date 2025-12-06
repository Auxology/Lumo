using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.Errors;
using Auth.Domain.Events.User;
using Auth.Domain.ValueObjects;
using SharedKernel.Authentication;
using SharedKernel.Constants;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Domain.Aggregates.UserAggregate;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<UserToken> _userTokens = [];
    
    public string DisplayName { get; private set; } = null!;
    
    public EmailAddress EmailAddress { get; private set; }
    
    public string? Avatar { get; private set; }
    
    public bool EmailVerified { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    public IReadOnlyCollection<UserToken> UserTokens => _userTokens;
    
    private User() { } // For EF Core

    [SetsRequiredMembers]
    private User
    (
        string displayName,
        EmailAddress emailAddress,
        DateTimeOffset utcNow
    )
    {
        Id = UserId.New();
        DisplayName = displayName;
        EmailAddress = emailAddress;
        Avatar = null;
        EmailVerified = false;
        CreatedAt = utcNow;
        UpdatedAt = null;
    }

    public static Result<User> Create
    (
        string displayName,
        EmailAddress emailAddress,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        if (string.IsNullOrWhiteSpace(displayName))
            return UserErrors.DisplayNameRequired;

        if (displayName.Length > UserConstants.MaxDisplayNameLength)
            return UserErrors.DisplayNameTooLong;
        
        if (emailAddress.IsEmpty())
            return UserErrors.EmailAddressRequired;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        User user = new
        (
            displayName: displayName,
            emailAddress: emailAddress,
            utcNow: utcNow
        );

        UserCreatedDomainEvent domainEvent = new
        (
            UserId: user.Id.Value,
            DisplayName: user.DisplayName,
            EmailAddress: user.EmailAddress.Value,
            OccurredOn: utcNow
        );
        
        user.AddDomainEvent(domainEvent);

        return user;
    }

    public Result ChangeName
    (
        string newDisplayName,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        if (string.IsNullOrWhiteSpace(newDisplayName))
            return UserErrors.DisplayNameRequired;

        if (newDisplayName.Length > UserConstants.MaxDisplayNameLength)
            return UserErrors.DisplayNameTooLong;
        
        if (DisplayName == newDisplayName)
            return Result.Success();
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        DisplayName = newDisplayName;
        UpdatedAt = utcNow;

        UserNameChangedDomainEvent domainEvent = new
        (
            UserId: Id.Value,
            NewDisplayName: newDisplayName,
            EmailAddress: EmailAddress.Value,
            OccurredOn: utcNow
        );

        AddDomainEvent(domainEvent);
        
        return Result.Success();
    }

    public Result RequestLogin
    (
        string otpToken,
        string otpTokenHash,
        string magicLinkToken,
        string magicLinkTokenHash,
        string? ipAddress,
        string? userAgent,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        if (string.IsNullOrWhiteSpace(otpToken))
            return UserErrors.OtpTokenRequired;

        if (string.IsNullOrWhiteSpace(magicLinkToken))
            return UserErrors.MagicLinkTokenRequired;

        string finalIpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? RequestConstants.IpAddressFallback
            : ipAddress;

        string finalUserAgent = string.IsNullOrWhiteSpace(userAgent)
            ? RequestConstants.UserAgentFallback
            : userAgent;

        Result<UserToken> userTokenResult = UserToken.Create
        (
            userId: Id,
            otpTokenHash: otpTokenHash,
            ipAddress: finalIpAddress,
            userAgent: finalUserAgent,
            magicLinkTokenHash: magicLinkTokenHash,
            dateTimeProvider: dateTimeProvider
        );

        if (userTokenResult.IsFailure)
            return userTokenResult.Error;
        
        _userTokens.Add(userTokenResult.Value);
        
        UserLoginRequestedDomainEvent domainEvent = new
        (
            UserId: Id.Value,
            EmailAddress: EmailAddress.Value,
            OtpToken: otpToken,
            MagicLinkToken: magicLinkToken,
            IpAddress: finalIpAddress,
            UserAgent: finalUserAgent,
            OccurredOn: dateTimeProvider.UtcNow
        );
        
        AddDomainEvent(domainEvent);
        
        return Result.Success();
    }

    public Result VerifyLogin
    (
        string? otpToken,
        string? magicLinkToken,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(secretHasher);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        UserToken? latestToken = _userTokens
            .Where(ut => !ut.IsUsed && !ut.IsExpired(dateTimeProvider))
            .OrderByDescending(ut => ut.CreatedAt)
            .FirstOrDefault();

        if (latestToken is null)
            return UserErrors.TokenNotFoundOrExpired;

        Result verifyResult = latestToken.Verify
        (
            otpToken: otpToken,
            magicLinkToken: magicLinkToken,
            secretHasher: secretHasher,
            dateTimeProvider: dateTimeProvider
        );
        
        if (verifyResult.IsFailure)
            return verifyResult.Error;
        
        return Result.Success();
    }
}