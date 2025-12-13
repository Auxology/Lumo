namespace Shared.Contracts.Authentication;

public sealed record CreateSessionRequest
(
    string EmailAddress,
    string? OtpToken,
    string? MagicLinkToken
);
