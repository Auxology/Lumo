using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.RefreshToken;

public sealed record RefreshTokenCommand
(
    string RefreshToken
) : ICommand<RefreshTokenResponse>;
