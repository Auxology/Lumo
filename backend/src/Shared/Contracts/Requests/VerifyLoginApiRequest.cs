namespace Contracts.Requests;

public sealed record VerifyLoginApiRequest
(
    string TokenKey,
    string? OtpToken,
    string? MagicLinkToken
);
