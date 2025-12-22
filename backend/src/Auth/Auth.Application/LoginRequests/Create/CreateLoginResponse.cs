namespace Auth.Application.LoginRequests.Create;

public sealed record CreateLoginResponse
(
    string TokenKey,
    DateTimeOffset ExpiresAt
);