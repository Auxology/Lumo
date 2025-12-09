using Shared.Contracts.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.VerifyLogin;

public sealed record VerifyLoginCommand
(
    string EmailAddress,
    string? OtpToken,
    string? MagicLinkToken
) : ICommand<VerifyLoginResponse>;