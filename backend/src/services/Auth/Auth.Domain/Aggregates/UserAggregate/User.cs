using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.Errors;
using Auth.Domain.Events.UserEvents;
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
    
    private readonly List<UserRecoveryCode> _userRecoveryCodes = [];
    
    public string DisplayName { get; private set; } = null!;
    
    public EmailAddress EmailAddress { get; private set; }
    
    public string? AvatarKey { get; private set; }
    
    public bool EmailVerified { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    public IReadOnlyCollection<UserToken> UserTokens => _userTokens;
    
    public IReadOnlyCollection<UserRecoveryCode> UserRecoveryCodes => _userRecoveryCodes;
    
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
        AvatarKey = null;
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

    public Result ChangeDisplayName
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

    public Result SetAvatar(string avatarKey, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        if (string.IsNullOrWhiteSpace(avatarKey))
            return UserErrors.AvatarKeyRequired;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        AvatarKey = avatarKey;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    public Result AddRecoveryCodes
    (
        IReadOnlyList<string> recoveryCodes,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(recoveryCodes);
        ArgumentNullException.ThrowIfNull(secretHasher);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        if (recoveryCodes.Count != RecoveryCodeConstants.CodesPerUser)
            return UserErrors.InvalidRecoveryCodeCount;

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        List<UserRecoveryCode> newRecoveryCodes = [];

        foreach (string recoveryCode in recoveryCodes)
        {
            Result<UserRecoveryCode> recoveryCodeResult = UserRecoveryCode.Create
            (
                userId: Id,
                recoveryCode: recoveryCode,
                secretHasher: secretHasher,
                dateTimeProvider: dateTimeProvider
            );

            if (recoveryCodeResult.IsFailure)
                return recoveryCodeResult.Error;

            newRecoveryCodes.Add(recoveryCodeResult.Value);
        }

        RevokeUnusedRecoveryCodes();
        
        _userRecoveryCodes.AddRange(newRecoveryCodes);
        
        UpdatedAt = utcNow;

        return Result.Success();
    }
    
    public Result RequestLogin
    (
        string otpToken,
        string magicLinkToken,
        string? ipAddress,
        string? userAgent,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(secretHasher);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        
        string finalIpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? RequestConstants.IpAddressFallback
            : ipAddress;

        string finalUserAgent = string.IsNullOrWhiteSpace(userAgent)
            ? RequestConstants.UserAgentFallback
            : userAgent;

        Result<UserToken> userTokenResult = UserToken.Create
        (
            userId: Id,
            otpToken: otpToken,
            ipAddress: finalIpAddress,
            userAgent: finalUserAgent,
            magicLinkToken: magicLinkToken,
            secretHasher: secretHasher,
            dateTimeProvider: dateTimeProvider
        );

        if (userTokenResult.IsFailure)
            return userTokenResult.Error;
        
        CleanupTokens(dateTimeProvider);
        
        _userTokens.Add(userTokenResult.Value);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        UpdatedAt = utcNow;
        
        UserLoginRequestedDomainEvent domainEvent = new
        (
            UserId: Id.Value,
            EmailAddress: EmailAddress.Value,
            OtpToken: otpToken,
            MagicLinkToken: magicLinkToken,
            IpAddress: finalIpAddress,
            UserAgent: finalUserAgent,
            OccurredOn: utcNow
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

        UserToken? latestToken = GetLatestValidToken(dateTimeProvider);

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

        if (!EmailVerified)
            EmailVerified = true;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        string verificationMethod = DetermineVerificationMethod(otpToken);

        UserLoginVerifiedDomainEvent domainEvent = new
        (
            UserId: Id.Value,
            EmailAddress: EmailAddress.Value,
            IpAddress: latestToken.IpAddress,
            UserAgent: latestToken.UserAgent,
            VerificationMethod: verificationMethod,
            OccurredOn: utcNow
        );
        
        AddDomainEvent(domainEvent);
        
        return Result.Success();
    }

    private void RevokeUnusedRecoveryCodes()
    {
        foreach (UserRecoveryCode userRecoveryCode in _userRecoveryCodes.Where(urc => urc.IsValid))
        {
            userRecoveryCode.Revoke();
        }
    }

    private UserToken? GetLatestValidToken(IDateTimeProvider dateTimeProvider)
    {
        return _userTokens
            .Where(ut => !ut.IsUsed && !ut.IsExpired(dateTimeProvider))
            .OrderByDescending(ut => ut.CreatedAt)
            .FirstOrDefault();
    }

    private void CleanupTokens(IDateTimeProvider dateTimeProvider)
    {
        DateTimeOffset cutoffTime = dateTimeProvider.UtcNow
            .AddMinutes(-UserConstants.TokenExpirationMinutes);

        _userTokens.RemoveAll(ut => ut.CreatedAt < cutoffTime);

        if (_userTokens.Count > UserConstants.MaxActiveTokens)
        {
            HashSet<UserToken> tokensToKeep = _userTokens
                .OrderByDescending(ut => ut.CreatedAt)
                .Take(UserConstants.MaxActiveTokens)
                .ToHashSet();

            _userTokens.RemoveAll(ut => !tokensToKeep.Contains(ut));
        }
    }
    
    private static string DetermineVerificationMethod(string? otpToken)
    {
        return !string.IsNullOrWhiteSpace(otpToken)
            ? UserConstants.OtpVerificationMethod
            : UserConstants.MagicLinkVerificationMethod;
    }
}