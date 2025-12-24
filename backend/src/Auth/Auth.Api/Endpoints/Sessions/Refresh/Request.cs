namespace Auth.Api.Endpoints.Sessions.Refresh;

internal sealed record Request
(
    string RefreshToken
);
