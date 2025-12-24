namespace Auth.Application.Sessions;

public sealed record RefreshTokenResponse
(
    string AccessToken,
    string RefreshToken
);
