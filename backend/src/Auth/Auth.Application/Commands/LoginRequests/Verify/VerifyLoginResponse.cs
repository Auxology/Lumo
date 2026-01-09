namespace Auth.Application.LoginRequests.Verify;

public sealed record VerifyLoginResponse
(
    string AccessToken,
    string RefreshToken
);