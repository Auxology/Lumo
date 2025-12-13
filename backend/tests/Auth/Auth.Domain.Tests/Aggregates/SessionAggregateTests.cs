using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.Events.SessionEvents;
using Auth.Domain.Tests.TestHelpers;
using Auth.Domain.ValueObjects;
using SharedKernel.ResultPattern;
using Shouldly;

namespace Auth.Domain.Tests.Aggregates;

public sealed class SessionTests
{
    private readonly FakeDateTimeProvider _dateTimeProvider = new();

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        UserId userId = UserId.New();
        
        const string hashedToken = "hashed-refresh-token";

        Result<Session> result = Session.Create
        (
            userId: userId,
            hashedSecret: hashedToken,
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            dateTimeProvider: _dateTimeProvider
        );
        
        Session session = result.Value;

        result.IsSuccess.ShouldBeTrue();
        
        session.UserId.ShouldBe(userId);
        
        session.HashedSecret.ShouldBe(hashedToken);
        
        session.Version.ShouldBe(1);
        
        session.IsRevoked.ShouldBeFalse();
    }

    [Fact]
    public void Create_ShouldSetExpirationDates()
    {
        UserId userId = UserId.New();

        Result<Session> result = Session.Create
        (
            userId: userId,
            hashedSecret: "token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            dateTimeProvider: _dateTimeProvider
        );

        Session session = result.Value;
        
        session.ExpiresAt.ShouldBe(_dateTimeProvider.UtcNow.AddDays(SessionConstants.ExpirationDays));
        
        session.AbsoluteExpiresAt.ShouldBe(_dateTimeProvider.UtcNow.AddDays(SessionConstants.AbsoluteExpirationDays));
    }

    [Fact]
    public void Create_ShouldRaiseSessionCreatedEvent()
    {
        UserId userId = UserId.New();

        Result<Session> result = Session.Create
        (
            userId: userId,
            hashedSecret: "token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            dateTimeProvider: _dateTimeProvider
        );
        
        Session session = result.Value;

        SessionCreatedDomainEvent createdEvent = result.Value.DomainEvents.First().ShouldBeOfType<SessionCreatedDomainEvent>();
        
        createdEvent.SessionId.ShouldBe(session.Id.Value);
        
        createdEvent.UserId.ShouldBe(userId.Value);

        createdEvent.IpAddress.ShouldBe("192.168.1.1");
        
        createdEvent.UserAgent.ShouldBe("Mozilla/5.0");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        UserId emptyUserId = UserId.UnsafeFromGuid(Guid.Empty);

        Result<Session> result = Session.Create
        (
            userId: emptyUserId,
            hashedSecret: "token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(SessionErrors.UserIdRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyHashedToken_ShouldFail(string? hashedToken)
    {
        Result<Session> result = Session.Create
        (
            userId: UserId.New(),
            hashedSecret: hashedToken!,
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(SessionErrors.HashedSecretRequired);
    }

    [Fact]
    public void Create_WithNullIpAndUserAgent_ShouldUseFallbacks()
    {
        Result<Session> result = Session.Create
        (
            userId: UserId.New(),
            hashedSecret: "token",
            ipAddress: null,
            userAgent: null,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsSuccess.ShouldBeTrue();
        
        result.Value.IpAddress.ShouldNotBeNullOrWhiteSpace();
        
        result.Value.UserAgent.ShouldNotBeNullOrWhiteSpace();
    }

    #endregion

    #region IsExpired Tests

    [Fact]
    public void IsExpired_BeforeExpirationDate_ShouldReturnFalse()
    {
        Session session = CreateValidSession();

        session.IsExpired(_dateTimeProvider).ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_AfterExpirationDate_ShouldReturnTrue()
    {
        Session session = CreateValidSession();
        
        _dateTimeProvider.Advance(TimeSpan.FromDays(SessionConstants.ExpirationDays + 1));

        session.IsExpired(_dateTimeProvider).ShouldBeTrue();
    }

    [Fact]
    public void IsExpired_AfterAbsoluteExpirationDate_ShouldReturnTrue()
    {
        Session session = CreateValidSession();
        
        _dateTimeProvider.Advance(TimeSpan.FromDays(SessionConstants.AbsoluteExpirationDays + 1));

        session.IsExpired(_dateTimeProvider).ShouldBeTrue();
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_ForNewSession_ShouldReturnTrue()
    {
        Session session = CreateValidSession();

        session.IsValid(_dateTimeProvider).ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenRevoked_ShouldReturnFalse()
    {
        Session session = CreateValidSession();
        
        session.Revoke(_dateTimeProvider);

        session.IsValid(_dateTimeProvider).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        Session session = CreateValidSession();
        
        _dateTimeProvider.Advance(TimeSpan.FromDays(SessionConstants.ExpirationDays + 1));

        session.IsValid(_dateTimeProvider).ShouldBeFalse();
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_ValidSession_ShouldSucceed()
    {
        Session session = CreateValidSession();
        
        session.ClearDomainEvents();

        Result result = session.Revoke(_dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        session.IsRevoked.ShouldBeTrue();
        
        session.RevokedAt.ShouldBe(_dateTimeProvider.UtcNow);
    }

    [Fact]
    public void Revoke_ShouldRaiseSessionRevokedEvent()
    {
        Session session = CreateValidSession();
        
        session.ClearDomainEvents();

        session.Revoke(_dateTimeProvider);

        SessionRevokedDomainEvent revokedEvent = session.DomainEvents.First().ShouldBeOfType<SessionRevokedDomainEvent>();
        
        revokedEvent.Reason.ShouldBe(SessionConstants.RevokedByUser);
        
        revokedEvent.SessionId.ShouldBe(session.Id.Value);
        
        revokedEvent.UserId.ShouldBe(session.UserId.Value);
    }

    [Fact]
    public void Revoke_AlreadyRevoked_ShouldFail()
    {
        Session session = CreateValidSession();
        
        session.Revoke(_dateTimeProvider);

        Result result = session.Revoke(_dateTimeProvider);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(SessionErrors.AlreadyRevoked);
    }

    [Fact]
    public void Revoke_ExpiredSession_ShouldSucceedWithoutEvent()
    {
        Session session = CreateValidSession();
        
        session.ClearDomainEvents();
        
        _dateTimeProvider.Advance(TimeSpan.FromDays(SessionConstants.ExpirationDays + 1));

        Result result = session.Revoke(_dateTimeProvider);

        result.IsSuccess.ShouldBeTrue();
        
        session.DomainEvents.ShouldBeEmpty();
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public void Refresh_ValidSession_ShouldSucceed()
    {
        Session session = CreateValidSession();
        
        session.ClearDomainEvents();
        const string newToken = "new-hashed-token";

        var result = session.Refresh
        (
            newHashedSecret: newToken,
            ipAddress: "10.0.0.1",
            userAgent: "Chrome",
            dateTimeProvider: _dateTimeProvider
        );

        result.IsSuccess.ShouldBeTrue();
        
        session.HashedSecret.ShouldBe(newToken);
        
        session.Version.ShouldBe(2);
        
        session.LastRefreshedAt.ShouldBe(_dateTimeProvider.UtcNow);
    }

    [Fact]
    public void Refresh_ShouldExtendExpiration()
    {
        Session session = CreateValidSession();
        
        _dateTimeProvider.Advance(TimeSpan.FromDays(10));
        
        DateTimeOffset expectedNewExpiration = _dateTimeProvider.UtcNow.AddDays(SessionConstants.ExpirationDays);

        session.Refresh
        (
            newHashedSecret: "new-token",
            ipAddress: null,
            userAgent: null,
            dateTimeProvider: _dateTimeProvider
        );

        session.ExpiresAt.ShouldBe(expectedNewExpiration);
    }

    [Fact]
    public void Refresh_ShouldRaiseSessionRefreshedEvent()
    {
        Session session = CreateValidSession();
        
        session.ClearDomainEvents();

        session.Refresh
        (
            newHashedSecret: "new-token",
            ipAddress: null,
            userAgent: null,
            dateTimeProvider: _dateTimeProvider
        );

        SessionRefreshedDomainEvent refreshedEvent = session.DomainEvents.First().ShouldBeOfType<SessionRefreshedDomainEvent>();
        
        refreshedEvent.SessionId.ShouldBe(session.Id.Value);
        
        refreshedEvent.UserId.ShouldBe(session.UserId.Value);
        
        refreshedEvent.NewVersion.ShouldBe(2);
        
        refreshedEvent.NewExpiresAt.ShouldBe(session.ExpiresAt);
    }

    [Fact]
    public void Refresh_RevokedSession_ShouldFail()
    {
        Session session = CreateValidSession();
        
        session.Revoke(_dateTimeProvider);

        Result result = session.Refresh
        (
            newHashedSecret: "new-token",
            ipAddress: null,
            userAgent: null,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(SessionErrors.Invalid);
    }

    [Fact]
    public void Refresh_ExpiredSession_ShouldFail()
    {
        Session session = CreateValidSession();
        
        _dateTimeProvider.Advance(TimeSpan.FromDays(SessionConstants.ExpirationDays + 1));

        Result result = session.Refresh
        (
            newHashedSecret: "new-token",
            ipAddress: null,
            userAgent: null,
            dateTimeProvider: _dateTimeProvider
        );

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(SessionErrors.Invalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Refresh_WithEmptyToken_ShouldFail(string? newToken)
    {
        Session session = CreateValidSession();

        Result result = session.Refresh
        (
            newHashedSecret: newToken!,
            ipAddress: null,
            userAgent: null,
            dateTimeProvider: _dateTimeProvider
        );
        
        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(SessionErrors.HashedSecretRequired);
    }

    #endregion

    #region Helper Methods

    private Session CreateValidSession()
    {
        return Session.Create
        (
            userId: UserId.New(),
            hashedSecret: "hashed-token",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            dateTimeProvider: _dateTimeProvider
        ).Value;
    }

    #endregion
}