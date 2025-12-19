using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;
using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class User : AggregateRoot<UserId>
{
    public string DisplayName { get; private set; } = string.Empty;
    
    public EmailAddress EmailAddress { get; private set; }
    
    public string? AvatarKey { get; private set; }
    
    public bool IsVerified { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    public DateTimeOffset? VerifiedAt { get; private set; }
    
    private User() {} // For EF Core

    [SetsRequiredMembers]
    public User
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
        IsVerified = false;
        CreatedAt = utcNow;
        UpdatedAt = null;
        VerifiedAt = null;
    }

    public static Outcome<User> Create
    (
        string displayName,
        EmailAddress emailAddress,
        DateTimeOffset utcNow
    )
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return UserFaults.DisplayNameRequiredForCreation;

        if (displayName.Length > UserConstants.MaxDisplayNameLength)
            return UserFaults.DisplayNameTooLongForCreation;

        if (emailAddress.IsEmpty())
            return UserFaults.EmailAddressRequiredForCreation;

        User user = new
        (
            displayName: displayName,
            emailAddress: emailAddress,
            utcNow: utcNow
        );

        return user;
    }

    public Outcome ChangeDisplayName(string newDisplayName, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            return UserFaults.DisplayNameRequiredForUpdate;

        if (newDisplayName.Length > UserConstants.MaxDisplayNameLength)
            return UserFaults.DisplayNameTooLongForUpdate;

        DisplayName = newDisplayName;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }

    public Outcome SetAvatarKey(string avatarKey, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
            return UserFaults.AvatarKeyRequiredForUpdate;
        
        AvatarKey = avatarKey;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }
    
    public Outcome RemoveAvatarKey(DateTimeOffset utcNow)
    {
        AvatarKey = null;
        UpdatedAt = utcNow;

        return Outcome.Success();
    }
}