using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.Aggregates;

public sealed class EmailChangeRequestTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly UserId ValidUserId = UserId.New();
    private static readonly EmailAddress ValidCurrentEmail = EmailAddress.UnsafeFromString("current@example.com");
    private static readonly EmailAddress ValidNewEmail = EmailAddress.UnsafeFromString("new@example.com");
    private const string ValidOtpTokenHash = "hashed-otp-token";
    private const string ValidEmailChangeRequestIdValue = "ecr_01JGX123456789012345678901";

    private static EmailChangeRequestId CreateValidEmailChangeRequestId() => EmailChangeRequestId.UnsafeFrom(ValidEmailChangeRequestIdValue);

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

        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
            currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: fingerprint,
            utcNow: UtcNow
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.CurrentEmailAddress.Should().Be(ValidCurrentEmail);
        outcome.Value.NewEmailAddress.Should().Be(ValidNewEmail);
        outcome.Value.OtpTokenHash.Should().Be(ValidOtpTokenHash);
        outcome.Value.Fingerprint.Should().Be(fingerprint);
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.ExpiresAt.Should().Be(UtcNow.AddMinutes(EmailChangeRequestConstants.ExpirationMinutes));
        outcome.Value.CompletedAt.Should().BeNull();
        outcome.Value.CancelledAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidData_ShouldUseProvidedId()
    {
        EmailChangeRequestId expectedId = CreateValidEmailChangeRequestId();

        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: expectedId,
            userId: ValidUserId,
            currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
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

        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: emptyUserId,
            currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.UserIdRequiredForCreation);
    }

    [Fact]
    public void Create_WithEmptyCurrentEmail_ShouldReturnFailure()
    {
        EmailAddress emptyEmail = EmailAddress.UnsafeFromString(string.Empty);

        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
            currentEmailAddress: emptyEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.CurrentEmailRequiredForCreation);
    }

    [Fact]
    public void Create_WithEmptyNewEmail_ShouldReturnFailure()
    {
        EmailAddress emptyEmail = EmailAddress.UnsafeFromString(string.Empty);

        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
            currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: emptyEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.NewEmailRequiredForCreation);
    }

    [Fact]
    public void Create_WithSameCurrentAndNewEmail_ShouldReturnFailure()
    {
        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
            currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidCurrentEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.EmailAddressesMustBeDifferent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOtpTokenHash_ShouldReturnFailure(string? otpTokenHash)
    {
        Outcome<EmailChangeRequest> outcome = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
            currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: otpTokenHash!,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.OtpTokenHashRequiredForCreation);
    }

    [Fact]
    public void Complete_WhenValid_ShouldSetCompletedAt()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset completeTime = UtcNow.AddMinutes(5);

        Outcome outcome = request.Complete(completeTime);

        outcome.IsSuccess.Should().BeTrue();
        request.CompletedAt.Should().Be(completeTime);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        request.Complete(UtcNow);

        Outcome outcome = request.Complete(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.AlreadyCompleted);
    }

    [Fact]
    public void Complete_WhenCancelled_ShouldReturnFailure()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        request.Cancel(UtcNow);

        Outcome outcome = request.Complete(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.AlreadyCancelled);
    }

    [Fact]
    public void Complete_WhenExpired_ShouldReturnFailure()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset expiredTime = UtcNow.AddMinutes(EmailChangeRequestConstants.ExpirationMinutes + 1);

        Outcome outcome = request.Complete(expiredTime);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.Expired);
    }

    [Fact]
    public void Complete_AtExactExpirationTime_ShouldReturnFailure()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset exactExpirationTime = UtcNow.AddMinutes(EmailChangeRequestConstants.ExpirationMinutes);

        Outcome outcome = request.Complete(exactExpirationTime);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.Expired);
    }

    [Fact]
    public void Complete_JustBeforeExpiration_ShouldSucceed()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset justBeforeExpiration = UtcNow.AddMinutes(EmailChangeRequestConstants.ExpirationMinutes).AddSeconds(-1);

        Outcome outcome = request.Complete(justBeforeExpiration);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenValid_ShouldSetCancelledAt()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset cancelTime = UtcNow.AddMinutes(5);

        Outcome outcome = request.Cancel(cancelTime);

        outcome.IsSuccess.Should().BeTrue();
        request.CancelledAt.Should().Be(cancelTime);
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        request.Complete(UtcNow);

        Outcome outcome = request.Cancel(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.AlreadyCompleted);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldReturnFailure()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        request.Cancel(UtcNow);

        Outcome outcome = request.Cancel(UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(EmailChangeRequestFaults.AlreadyCancelled);
    }

    [Fact]
    public void IsActive_WhenNotCompletedNotCancelledNotExpired_ShouldReturnTrue()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        bool isActive = request.IsActive(UtcNow);

        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenCompleted_ShouldReturnFalse()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        request.Complete(UtcNow);

        bool isActive = request.IsActive(UtcNow);

        isActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenCancelled_ShouldReturnFalse()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        request.Cancel(UtcNow);

        bool isActive = request.IsActive(UtcNow);

        isActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        EmailChangeRequest request = EmailChangeRequest.Create
        (
            id: CreateValidEmailChangeRequestId(),
            userId: ValidUserId,
                        currentEmailAddress: ValidCurrentEmail,
            newEmailAddress: ValidNewEmail,
            otpTokenHash: ValidOtpTokenHash,
            fingerprint: CreateValidFingerprint(),
            utcNow: UtcNow
        ).Value;

        DateTimeOffset expiredTime = UtcNow.AddMinutes(EmailChangeRequestConstants.ExpirationMinutes + 1);

        bool isActive = request.IsActive(expiredTime);

        isActive.Should().BeFalse();
    }
}