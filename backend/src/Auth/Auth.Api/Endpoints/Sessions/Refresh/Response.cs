namespace Auth.Api.Endpoints.Sessions.Refresh;

internal sealed record Response
(
    string AccessToken,
    string RefreshToken
);
