namespace Auth.Api.Endpoints.LoginRequests.Verify;

internal sealed record Response
(
    string AccessToken,
    string RefreshToken
);