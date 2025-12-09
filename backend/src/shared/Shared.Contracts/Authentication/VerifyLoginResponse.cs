namespace Shared.Contracts.Authentication;

public sealed record VerifyLoginResponse
(
    string AccessToken,
    string RefreshToken
);