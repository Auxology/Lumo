namespace Auth.Api.Endpoints.RecoveryRequests.Verify;

internal sealed record Request
(
    string TokenKey,      // From route parameter
    string? OtpToken,     // From request body
    string? MagicLinkToken // From request body
);