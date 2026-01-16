using System.Diagnostics.CodeAnalysis;

using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;

using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class EmailChangeRequest : AggregateRoot<EmailChangeRequestId>
{
    public UserId UserId { get; private set; }

    public string TokenKey { get; private set; } = string.Empty;

    public EmailAddress CurrentEmailAddress { get; private set; }

    public EmailAddress NewEmailAddress { get; private set; }

    public string OtpTokenHash { get; private set; } = string.Empty;

    public Fingerprint Fingerprint { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    private EmailChangeRequest() { } // For EF Core

    [SetsRequiredMembers]
    private EmailChangeRequest
    (
        EmailChangeRequestId id,
        UserId userId,
        string tokenKey,
        EmailAddress currentEmailAddress,
        EmailAddress newEmailAddress,
        string otpTokenHash,
        Fingerprint fingerprint,
        DateTimeOffset utcNow
    )
    {
        Id = id;
        UserId = userId;
        TokenKey = tokenKey;
        CurrentEmailAddress = currentEmailAddress;
        NewEmailAddress = newEmailAddress;
        OtpTokenHash = otpTokenHash;
        Fingerprint = fingerprint;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(EmailChangeRequestConstants.ExpirationMinutes);
        CompletedAt = null;
        CancelledAt = null;
    }

    public static Outcome<EmailChangeRequest> Create
    (
        EmailChangeRequestId id,
        UserId userId,
        string tokenKey,
        EmailAddress currentEmailAddress,
        EmailAddress newEmailAddress,
        string otpTokenHash,
        Fingerprint fingerprint,
        DateTimeOffset utcNow
    )
    {
        if (userId.IsEmpty)
            return EmailChangeRequestFaults.UserIdRequiredForCreation;

        if (string.IsNullOrWhiteSpace(tokenKey))
            return EmailChangeRequestFaults.TokenKeyRequiredForCreation;

        if (currentEmailAddress.IsEmpty())
            return EmailChangeRequestFaults.CurrentEmailRequiredForCreation;

        if (newEmailAddress.IsEmpty())
            return EmailChangeRequestFaults.NewEmailRequiredForCreation;

        if (currentEmailAddress == newEmailAddress)
            return EmailChangeRequestFaults.EmailAddressesMustBeDifferent;

        if (string.IsNullOrWhiteSpace(otpTokenHash))
            return EmailChangeRequestFaults.OtpTokenHashRequiredForCreation;

        EmailChangeRequest emailChangeRequest = new
        (
            id: id,
            userId: userId,
            tokenKey: tokenKey,
            currentEmailAddress: currentEmailAddress,
            newEmailAddress: newEmailAddress,
            otpTokenHash: otpTokenHash,
            fingerprint: fingerprint,
            utcNow: utcNow
        );

        return emailChangeRequest;
    }

    public Outcome Complete(DateTimeOffset utcNow)
    {
        if (IsCompleted)
            return EmailChangeRequestFaults.AlreadyCompleted;

        if (IsCancelled)
            return EmailChangeRequestFaults.AlreadyCancelled;

        if (IsExpired(utcNow))
            return EmailChangeRequestFaults.Expired;

        CompletedAt = utcNow;

        return Outcome.Success();
    }

    public Outcome Cancel(DateTimeOffset utcNow)
    {
        if (IsCompleted)
            return EmailChangeRequestFaults.AlreadyCompleted;

        if (IsCancelled)
            return EmailChangeRequestFaults.AlreadyCancelled;

        CancelledAt = utcNow;

        return Outcome.Success();
    }

    private bool IsCompleted => CompletedAt is not null;

    private bool IsCancelled => CancelledAt is not null;

    private bool IsExpired(DateTimeOffset utcNow) => ExpiresAt <= utcNow;

    public bool IsActive(DateTimeOffset utcNow) =>
        !IsCompleted && !IsCancelled && !IsExpired(utcNow);
}