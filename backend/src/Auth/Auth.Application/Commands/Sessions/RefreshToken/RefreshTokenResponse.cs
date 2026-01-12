namespace Auth.Application.Commands.Sessions.RefreshToken;

public sealed record RefreshTokenResponse
(
    string AccessToken,
    string RefreshToken
);