namespace Auth.Application.Commands.LoginRequests.Verify;

public sealed record VerifyLoginResponse
(
    string AccessToken,
    string RefreshToken
);