namespace Auth.Api.Endpoints.LoginRequests.Verify;

internal sealed record Request
(
    string TokenKey,
    string? OtpToken,
    string? MagicLinkToken
);