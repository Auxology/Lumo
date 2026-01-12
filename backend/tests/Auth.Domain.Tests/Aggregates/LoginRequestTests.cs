using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.Aggregates;

public sealed class LoginRequestTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly UserId ValidUserId = UserId.New();
    private const string ValidTokenKey = "token-key-123";
    private const string ValidOtpTokenHash = "hashed-otp-token";
    private const string ValidMagicLinkTokenHash = "hashed-magic-link-token";
    private const string ValidLoginRequestIdValue = "lrq_01JGX123456789012345678901";

    private static LoginRequestId CreateValidLoginRequestId() => LoginRequestId.UnsafeFrom(ValidLoginRequestIdValue);

    private static Fingerprint CreateValidFingerprint()
    {
        return Fingerprint.Create
        (
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            timezone: "Europe/London",
            language: "en-US",
            normalizedBrowser: "Chrome 120",
            normalizedOs: "Windows 11"
        ).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        Fingerprint fingerprint = CreateValidFingerprint();

        Outcome<LoginRequest> outcome = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: fingerprint,
            utcNow: UtcNow
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.TokenKey.Should().Be(ValidTokenKey);
        outcome.Value.OtpTokenHash.Should().Be(ValidOtpTokenHash);
        outcome.Value.MagicLinkTokenHash.Should().Be(ValidMagicLinkTokenHash);
        outcome.Value.Fingerprint.Should().Be(fingerprint);
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.ExpiresAt.Should().Be(UtcNow.AddMinutes(LoginRequestConstants.ExpirationMinutes));
        outcome.Value.ConsumedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidData_ShouldUseProvidedId()
    {
        LoginRequestId expectedId = CreateValidLoginRequestId();

        Outcome<LoginRequest> outcome = LoginRequest.Create
        (
            id: expectedId,
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        UserId emptyUserId = UserId.UnsafeFromGuid(Guid.Empty);

        Outcome<LoginRequest> outcome = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: emptyUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.UserIdRequiredForCreation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyTokenKey_ShouldReturnFailure(string? tokenKey)
    {
        Outcome<LoginRequest> outcome = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: tokenKey!,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.TokenKeyRequiredForCreation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOtpTokenHash_ShouldReturnFailure(string? otpTokenHash)
    {
        Outcome<LoginRequest> outcome = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: otpTokenHash!,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.OtpTokenHashRequiredForCreation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyMagicLinkTokenHash_ShouldReturnFailure(string? magicLinkTokenHash)
    {
        Outcome<LoginRequest> outcome = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: magicLinkTokenHash!,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.MagicLinkTokenHashRequiredForCreation);
    }

    [Fact]
    public void Consume_WhenValid_ShouldSetConsumedAt()
    {
        LoginRequest loginRequest = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset consumeTime = UtcNow.AddMinutes(5);

        Outcome outcome = loginRequest.Consume(consumeTime);

        outcome.IsSuccess.Should().BeTrue();
        loginRequest.ConsumedAt.Should().Be(consumeTime);
    }

    [Fact]
    public void Consume_WhenAlreadyConsumed_ShouldReturnFailure()
    {
        LoginRequest loginRequest = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        loginRequest.Consume(UtcNow);

        Outcome outcome = loginRequest.Consume(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.InvalidOrExpired);
    }

    [Fact]
    public void Consume_WhenExpired_ShouldReturnFailure()
    {
        LoginRequest loginRequest = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset expiredTime = UtcNow.AddMinutes(LoginRequestConstants.ExpirationMinutes + 1);

        Outcome outcome = loginRequest.Consume(expiredTime);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.InvalidOrExpired);
    }

    [Fact]
    public void Consume_AtExactExpirationTime_ShouldReturnFailure()
    {
        LoginRequest loginRequest = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset exactExpirationTime = UtcNow.AddMinutes(LoginRequestConstants.ExpirationMinutes);

        Outcome outcome = loginRequest.Consume(exactExpirationTime);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(LoginRequestFaults.InvalidOrExpired);
    }

    [Fact]
    public void Consume_JustBeforeExpiration_ShouldSucceed()
    {
        LoginRequest loginRequest = LoginRequest.Create
        (
            id: CreateValidLoginRequestId(),
            userId: ValidUserId,
            tokenKey: ValidTokenKey,
            otpTokenHash: ValidOtpTokenHash,
            magicLinkTokenHash: ValidMagicLinkTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset justBeforeExpiration = UtcNow.AddMinutes(LoginRequestConstants.ExpirationMinutes).AddSeconds(-1);

        Outcome outcome = loginRequest.Consume(justBeforeExpiration);

        outcome.IsSuccess.Should().BeTrue();
    }
}