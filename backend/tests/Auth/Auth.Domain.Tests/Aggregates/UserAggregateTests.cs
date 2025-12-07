using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.Events.UserEvents;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using Shouldly;

namespace Auth.Domain.Tests.Aggregates;

public sealed class UserTests
{
    private readonly FakeDateTimeProvider _dateTimeProvider = new();
    private readonly FakeSecretHasher _secretHasher = new();

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        const string displayName = "John Doe";
        
        EmailAddress email = EmailAddress.Create("john@example.com").Value;

        Result<User> result = User.Create(displayName, email, _dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        result.Value.DisplayName.ShouldBe(displayName);
        
        result.Value.EmailAddress.ShouldBe(email);
        
        result.Value.EmailVerified.ShouldBeFalse();
        
        result.Value.AvatarKey.ShouldBeNull();
        
        result.Value.CreatedAt.ShouldBe(_dateTimeProvider.UtcNow);
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedEvent()
    {
        const string displayName = "John Doe";
        
        EmailAddress email = EmailAddress.Create("john@example.com").Value;

        Result<User> result = User.Create(displayName, email, _dateTimeProvider);
        
        User user = result.Value;

        IReadOnlyCollection<IDomainEvent> domainEvents = result.Value.DomainEvents;
        
        domainEvents.ShouldHaveSingleItem();
        
        UserCreatedDomainEvent createdEvent = domainEvents.First().ShouldBeOfType<UserCreatedDomainEvent>();
        
        createdEvent.UserId.ShouldBe(user.Id.Value);
        
        createdEvent.DisplayName.ShouldBe(displayName);
        
        createdEvent.EmailAddress.ShouldBe(email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDisplayName_ShouldFail(string? displayName)
    {
        EmailAddress email = EmailAddress.Create("john@example.com").Value;

        Result<User> result = User.Create(displayName!, email, _dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.DisplayNameRequired);
    }

    [Fact]
    public void Create_WithTooLongDisplayName_ShouldFail()
    {
        string displayName = new('a', UserConstants.MaxDisplayNameLength + 1);
        
        EmailAddress email = EmailAddress.Create("john@example.com").Value;

        Result<User> result = User.Create(displayName, email, _dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.DisplayNameTooLong);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldFail()
    {
        EmailAddress email = default;

        Result<User> result = User.Create("John", email, _dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.EmailAddressRequired);
    }

    #endregion

    #region ChangeName Tests

    [Fact]
    public void ChangeName_WithValidName_ShouldSucceed()
    {
        var user = CreateValidUser();
        
        const string newName = "Jane Doe";

        Result result = user.ChangeName(newName, _dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        user.DisplayName.ShouldBe(newName);
        
        user.UpdatedAt.ShouldBe(_dateTimeProvider.UtcNow);
    }

    [Fact]
    public void ChangeName_ShouldRaiseNameChangedEvent()
    {
        User user = CreateValidUser();
        
        user.ClearDomainEvents();
        
        const string newName = "Jane Doe";

        user.ChangeName(newName, _dateTimeProvider);

        IReadOnlyCollection<IDomainEvent> domainEvents = user.DomainEvents;
        
        domainEvents.ShouldHaveSingleItem();
        
        UserNameChangedDomainEvent nameChangedEvent = domainEvents.First().ShouldBeOfType<UserNameChangedDomainEvent>();
        
        nameChangedEvent.UserId.ShouldBe(user.Id.Value);
        
        nameChangedEvent.NewDisplayName.ShouldBe(newName);
        
        nameChangedEvent.EmailAddress.ShouldBe(user.EmailAddress.Value);
    }

    [Fact]
    public void ChangeName_ToSameName_ShouldSucceedWithoutEvent()
    {
        User user = CreateValidUser();
        
        user.ClearDomainEvents();
        
        string sameName = user.DisplayName;

        Result result = user.ChangeName(sameName, _dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        user.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_WithEmptyName_ShouldFail(string? newName)
    {
        User user = CreateValidUser();

        Result result = user.ChangeName(newName!, _dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.DisplayNameRequired);
    }

    #endregion

    #region SetAvatar Tests

    [Fact]
    public void SetAvatar_WithValidKey_ShouldSucceed()
    {
        User user = CreateValidUser();
        
        const string avatarKey = "avatars/user123.jpg";
        
        Result result = user.SetAvatar(avatarKey, _dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        user.AvatarKey.ShouldBe(avatarKey);
        
        user.UpdatedAt.ShouldBe(_dateTimeProvider.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetAvatar_WithEmptyKey_ShouldFail(string? avatarKey)
    {
        User user = CreateValidUser();

        Result result = user.SetAvatar(avatarKey!, _dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.AvatarKeyRequired);
    }

    #endregion

    #region RequestLogin Tests

    [Fact]
    public void RequestLogin_WithValidData_ShouldSucceed()
    {
        User user = CreateValidUser();
        
        user.ClearDomainEvents();

        Result result = user.RequestLogin
        (
            otpToken: "123456",
            magicLinkToken: "magic-token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsSuccess.ShouldBeTrue();
        
        user.UserTokens.Count.ShouldBe(1);
    }

    [Fact]
    public void RequestLogin_ShouldRaiseLoginRequestedEvent()
    {
        User user = CreateValidUser();
        
        user.ClearDomainEvents();

        user.RequestLogin
        (
            otpToken: "123456",
            magicLinkToken: "magic-token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );
        
        UserLoginRequestedDomainEvent loginEvent = user.DomainEvents.First().ShouldBeOfType<UserLoginRequestedDomainEvent>();
        
        loginEvent.UserId.ShouldBe(user.Id.Value);
        
        loginEvent.EmailAddress.ShouldBe(user.EmailAddress.Value);
        
        loginEvent.OtpToken.ShouldBe("123456");
        
        loginEvent.MagicLinkToken.ShouldBe("magic-token");
        
        loginEvent.IpAddress.ShouldBe("192.168.1.1");
        
        loginEvent.UserAgent.ShouldBe("Mozilla/5.0");
    }

    [Fact]
    public void RequestLogin_WithNullIpAddress_ShouldUseFallback()
    {
        User user = CreateValidUser();
        
        user.ClearDomainEvents();

        user.RequestLogin
        (
            otpToken: "123456",
            magicLinkToken: "magic-token",
            ipAddress: null,
            userAgent: null,
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        UserLoginRequestedDomainEvent loginEvent = user.DomainEvents.First().ShouldBeOfType<UserLoginRequestedDomainEvent>();
        
        loginEvent.IpAddress.ShouldNotBeNullOrWhiteSpace();
        
        loginEvent.UserAgent.ShouldNotBeNullOrWhiteSpace();
    }

    #endregion

    #region VerifyLogin Tests

    [Fact]
    public void VerifyLogin_WithValidOtp_ShouldSucceed()
    {
        User user = CreateUserWithLoginToken();
        
        user.ClearDomainEvents();

        var result = user.VerifyLogin
        (
            otpToken: "123456",
            magicLinkToken: null,
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsSuccess.ShouldBeTrue();
        
        user.EmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void VerifyLogin_WithValidMagicLink_ShouldSucceed()
    {
        User user = CreateUserWithLoginToken();
        
        user.ClearDomainEvents();

        var result = user.VerifyLogin
        (
            otpToken: null,
            magicLinkToken: "magic-token",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsSuccess.ShouldBeTrue();
        
        user.EmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void VerifyLogin_ShouldRaiseLoginVerifiedEvent()
    {
        User user = CreateUserWithLoginToken();
        
        user.ClearDomainEvents();

        user.VerifyLogin
        (
            otpToken: "123456",
            magicLinkToken: null,
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        UserLoginVerifiedDomainEvent verifiedEvent = user.DomainEvents.First().ShouldBeOfType<UserLoginVerifiedDomainEvent>();
        
        verifiedEvent.VerificationMethod.ShouldBe(UserConstants.OtpVerificationMethod);
        
        verifiedEvent.UserId.ShouldBe(user.Id.Value);
        
        verifiedEvent.EmailAddress.ShouldBe(user.EmailAddress.Value);
        
        verifiedEvent.IpAddress.ShouldNotBeNullOrWhiteSpace();
        
        verifiedEvent.UserAgent.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void VerifyLogin_WithInvalidToken_ShouldFail()
    {
        User user = CreateUserWithLoginToken();

        Result result = user.VerifyLogin
        (
            otpToken: "wrong-otp",
            magicLinkToken: null,
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserTokenErrors.InvalidToken);
    }

    [Fact]
    public void VerifyLogin_WithExpiredToken_ShouldFail()
    {
        User user = CreateUserWithLoginToken();
        
        _dateTimeProvider.Advance(TimeSpan.FromMinutes(UserConstants.TokenExpirationMinutes + 1));

        Result result = user.VerifyLogin
        (
            otpToken: "123456",
            magicLinkToken: null,
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.TokenNotFoundOrExpired);
    }

    [Fact]
    public void VerifyLogin_WithNoToken_ShouldFail()
    {
        User user = CreateValidUser();

        var result = user.VerifyLogin
        (
            otpToken: "123456",
            magicLinkToken: null,
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.TokenNotFoundOrExpired);
    }

    #endregion

    #region AddRecoveryCodes Tests

    [Fact]
    public void AddRecoveryCodes_WithValidCodes_ShouldSucceed()
    {
        User user = CreateValidUser();
        
        List<string> codes = GenerateRecoveryCodes(RecoveryCodeConstants.CodesPerUser);

        Result result = user.AddRecoveryCodes(codes, _secretHasher, _dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        user.UserRecoveryCodes.Count.ShouldBe(RecoveryCodeConstants.CodesPerUser);
    }

    [Fact]
    public void AddRecoveryCodes_WithWrongCount_ShouldFail()
    {
        User user = CreateValidUser();
        
        List<string> codes = GenerateRecoveryCodes(RecoveryCodeConstants.CodesPerUser - 1);

        Result result = user.AddRecoveryCodes(codes, _secretHasher, _dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserErrors.InvalidCodeCount);
    }

    [Fact]
    public void AddRecoveryCodes_ShouldRevokeExistingCodes()
    {
        User user = CreateValidUser();
        
        List<string> firstCodes = GenerateRecoveryCodes(RecoveryCodeConstants.CodesPerUser);
        
        user.AddRecoveryCodes(firstCodes, _secretHasher, _dateTimeProvider);
        
        List<string> secondCodes = GenerateRecoveryCodes(RecoveryCodeConstants.CodesPerUser);

        user.AddRecoveryCodes(secondCodes, _secretHasher, _dateTimeProvider);

        user.UserRecoveryCodes.Count.ShouldBe(RecoveryCodeConstants.CodesPerUser * 2);
        
        user.UserRecoveryCodes.Count(c => c.IsRevoked).ShouldBe(RecoveryCodeConstants.CodesPerUser);
    }

    #endregion

    #region Helper Methods

    private User CreateValidUser()
    {
        EmailAddress email = EmailAddress.Create("john@example.com").Value;
        return User.Create("John Doe", email, _dateTimeProvider).Value;
    }

    private User CreateUserWithLoginToken()
    {
        User user = CreateValidUser();
        
        user.RequestLogin
        (
            otpToken: "123456",
            magicLinkToken: "magic-token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            secretHasher: _secretHasher,
            dateTimeProvider: _dateTimeProvider
        );
        
        return user;
    }

    private static List<string> GenerateRecoveryCodes(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => $"CODE{i:D4}")
            .ToList();
    }

    #endregion
}