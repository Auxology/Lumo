namespace Auth.Api.Endpoints.LoginRequests.Create;

internal sealed record Response
(
    string TokenKey,
    DateTimeOffset ExpiresAt
);