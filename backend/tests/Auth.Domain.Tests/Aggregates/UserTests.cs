using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.Events.User;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.Aggregates;

public sealed class UserTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly EmailAddress ValidEmail = EmailAddress.UnsafeFromString("test@example.com");

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        string displayName = "John Doe";

        Outcome<User> outcome = User.Create(displayName, ValidEmail, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.DisplayName.Should().Be(displayName);
        outcome.Value.EmailAddress.Should().Be(ValidEmail);
        outcome.Value.IsVerified.Should().BeFalse();
        outcome.Value.AvatarKey.Should().BeNull();
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.UpdatedAt.Should().BeNull();
        outcome.Value.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidData_ShouldGenerateNewId()
    {
        Outcome<User> outcome = User.Create("John Doe", ValidEmail, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseDomainEvent()
    {
        Outcome<User> outcome = User.Create("John Doe", ValidEmail, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.DomainEvents.Should().ContainSingle();
        outcome.Value.DomainEvents.First().Should().BeOfType<UserCreatedDomainEvent>();
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseDomainEventWithCorrectData()
    {
        string displayName = "John Doe";

        Outcome<User> outcome = User.Create(displayName, ValidEmail, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        UserCreatedDomainEvent? domainEvent = outcome.Value.DomainEvents.First() as UserCreatedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.UserId.Should().Be(outcome.Value.Id.Value);
        domainEvent.EmailAddress.Should().Be(ValidEmail.Value);
        domainEvent.CreatedAt.Should().Be(UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDisplayName_ShouldReturnFailure(string? displayName)
    {
        Outcome<User> outcome = User.Create(displayName!, ValidEmail, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.DisplayNameRequiredForCreation);
    }

    [Fact]
    public void Create_WithTooLongDisplayName_ShouldReturnFailure()
    {
        string displayName = new('a', UserConstants.MaxDisplayNameLength + 1);

        Outcome<User> outcome = User.Create(displayName, ValidEmail, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.DisplayNameTooLongForCreation);
    }

    [Fact]
    public void Create_WithEmptyEmailAddress_ShouldReturnFailure()
    {
        EmailAddress emptyEmail = EmailAddress.UnsafeFromString(string.Empty);

        Outcome<User> outcome = User.Create("John Doe", emptyEmail, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.EmailAddressRequiredForCreation);
    }

    [Fact]
    public void ChangeDisplayName_WithValidName_ShouldUpdateDisplayName()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        string newDisplayName = "Jane Doe";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = user.ChangeDisplayName(newDisplayName, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        user.DisplayName.Should().Be(newDisplayName);
        user.UpdatedAt.Should().Be(updateTime);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangeDisplayName_WithEmptyName_ShouldReturnFailure(string? newDisplayName)
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;

        Outcome outcome = user.ChangeDisplayName(newDisplayName!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.DisplayNameRequiredForUpdate);
    }

    [Fact]
    public void ChangeDisplayName_WithTooLongName_ShouldReturnFailure()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        string newDisplayName = new('a', UserConstants.MaxDisplayNameLength + 1);

        Outcome outcome = user.ChangeDisplayName(newDisplayName, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.DisplayNameTooLongForUpdate);
    }

    [Fact]
    public void SetAvatarKey_WithValidKey_ShouldUpdateAvatarKey()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        string avatarKey = "avatar-key-123";
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = user.SetAvatarKey(avatarKey, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        user.AvatarKey.Should().Be(avatarKey);
        user.UpdatedAt.Should().Be(updateTime);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void SetAvatarKey_WithEmptyKey_ShouldReturnFailure(string? avatarKey)
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;

        Outcome outcome = user.SetAvatarKey(avatarKey!, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.AvatarKeyRequiredForUpdate);
    }

    [Fact]
    public void RemoveAvatarKey_ShouldClearAvatarKey()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        user.SetAvatarKey("avatar-key-123", UtcNow);
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        user.RemoveAvatarKey(updateTime);

        user.AvatarKey.Should().BeNull();
        user.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void ChangeEmailAddress_WithValidEmail_ShouldUpdateEmailAddress()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        EmailAddress newEmail = EmailAddress.UnsafeFromString("new@example.com");
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = user.ChangeEmailAddress(newEmail, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        user.EmailAddress.Should().Be(newEmail);
        user.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void ChangeEmailAddress_WithValidEmail_ShouldResetVerificationStatus()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        EmailAddress newEmail = EmailAddress.UnsafeFromString("new@example.com");
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = user.ChangeEmailAddress(newEmail, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        user.IsVerified.Should().BeFalse();
        user.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public void ChangeEmailAddress_WithEmptyEmail_ShouldReturnFailure()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        EmailAddress emptyEmail = EmailAddress.UnsafeFromString(string.Empty);

        Outcome outcome = user.ChangeEmailAddress(emptyEmail, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.EmailAddressRequiredForUpdate);
    }

    [Fact]
    public void ChangeEmailAddress_WithSameEmail_ShouldReturnFailure()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;

        Outcome outcome = user.ChangeEmailAddress(ValidEmail, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.EmailAddressSameAsCurrent);
    }

    [Fact]
    public void MarkAsDeleted_WhenNotDeleted_ShouldSetDeletedAt()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        DateTimeOffset deleteTime = UtcNow.AddHours(1);

        Outcome outcome = user.MarkAsDeleted(deleteTime);

        outcome.IsSuccess.Should().BeTrue();
        user.DeletedAt.Should().Be(deleteTime);
        user.UpdatedAt.Should().Be(deleteTime);
    }

    [Fact]
    public void MarkAsDeleted_WhenAlreadyDeleted_ShouldReturnFailure()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        user.MarkAsDeleted(UtcNow);

        Outcome outcome = user.MarkAsDeleted(UtcNow.AddHours(1));

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.UserAlreadyDeleted);
    }

    [Fact]
    public void CancelDeletion_WhenDeleted_ShouldClearDeletedAt()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;
        user.MarkAsDeleted(UtcNow);
        DateTimeOffset cancelTime = UtcNow.AddHours(1);

        Outcome outcome = user.CancelDeletion(cancelTime);

        outcome.IsSuccess.Should().BeTrue();
        user.DeletedAt.Should().BeNull();
        user.UpdatedAt.Should().Be(cancelTime);
    }

    [Fact]
    public void CancelDeletion_WhenNotDeleted_ShouldReturnFailure()
    {
        User user = User.Create("John Doe", ValidEmail, UtcNow).Value;

        Outcome outcome = user.CancelDeletion(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(UserFaults.UserNotDeleted);
    }
}