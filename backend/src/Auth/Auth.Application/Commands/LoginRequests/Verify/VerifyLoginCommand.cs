using SharedKernel.Application.Messaging;

namespace Auth.Application.LoginRequests.Verify;

public sealed record VerifyLoginCommand
(
    string TokenKey,
    string? OtpToken,
    string? MagicLinkToken
) : ICommand<VerifyLoginResponse>;