using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.Events.User;
using Auth.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Domain.Aggregates.UserAggregate;

public sealed class User : AggregateRoot<UserId>
{
    public string DisplayName { get; private set; } = null!;
    
    public EmailAddress EmailAddress { get; private set; }
    
    public string? Avatar { get; private set; }
    
    public bool EmailVerified { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
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
}