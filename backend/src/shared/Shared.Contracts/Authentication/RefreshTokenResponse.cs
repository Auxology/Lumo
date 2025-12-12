namespace Shared.Contracts.Authentication;

public sealed record RefreshTokenResponse
(
    string AccessToken,
    string RefreshToken
);
