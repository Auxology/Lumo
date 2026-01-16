using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.EmailChangeRequests.Verify;

public sealed record VerifyEmailChangeCommand
(
    string TokenKey,
    string OtpToken
) : ICommand<VerifyEmailChangeResponse>;