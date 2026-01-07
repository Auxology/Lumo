using System.Diagnostics.CodeAnalysis;

using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;

using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class LoginRequest : AggregateRoot<LoginRequestId>
{
    public UserId UserId { get; private set; }

    public string TokenKey { get; private set; } = string.Empty;

    public string OtpTokenHash { get; private set; } = string.Empty;

    public string MagicLinkTokenHash { get; private set; } = string.Empty;

    public Fingerprint Fingerprint { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    private LoginRequest() { } // For EF Core

    [SetsRequiredMembers]
    private LoginRequest
    (
        UserId userId,
        string tokenKey,
        string otpTokenHash,
        string magicLinkTokenHash,
        Fingerprint fingerprint,
        DateTimeOffset utcNow
    )
    {
        Id = LoginRequestId.New();
        UserId = userId;
        TokenKey = tokenKey;
        OtpTokenHash = otpTokenHash;
        MagicLinkTokenHash = magicLinkTokenHash;
        Fingerprint = fingerprint;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(LoginRequestConstants.ExpirationMinutes);
        ConsumedAt = null;
    }

    public static Outcome<LoginRequest> Create
    (
        UserId userId,
        string tokenKey,
        string otpTokenHash,
        string magicLinkTokenHash,
        Fingerprint fingerprint,
        DateTimeOffset utcNow
    )
    {
        if (userId.IsEmpty)
            return LoginRequestFaults.UserIdRequiredForCreation;

        if (string.IsNullOrWhiteSpace(tokenKey))
            return LoginRequestFaults.TokenKeyRequiredForCreation;

        if (string.IsNullOrWhiteSpace(otpTokenHash))
            return LoginRequestFaults.OtpTokenHashRequiredForCreation;

        if (string.IsNullOrWhiteSpace(magicLinkTokenHash))
            return LoginRequestFaults.MagicLinkTokenHashRequiredForCreation;

        LoginRequest loginRequest = new
        (
            userId,
            tokenKey,
            otpTokenHash,
            magicLinkTokenHash,
            fingerprint,
            utcNow
        );

        return loginRequest;
    }

    public Outcome Consume(DateTimeOffset utcNow)
    {
        if (ConsumedAt is not null)
            return LoginRequestFaults.InvalidOrExpired;

        if (ExpiresAt <= utcNow)
            return LoginRequestFaults.InvalidOrExpired;

        ConsumedAt = utcNow;

        return Outcome.Success();
    }

}