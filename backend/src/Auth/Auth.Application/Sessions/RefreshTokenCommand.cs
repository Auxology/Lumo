using SharedKernel.Application.Messaging;

namespace Auth.Application.Sessions;

public sealed record RefreshTokenCommand
(
    string RefreshToken
) : ICommand<RefreshTokenResponse>;