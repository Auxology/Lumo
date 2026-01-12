namespace Auth.Application.Commands.LoginRequests.Create;

public sealed record CreateLoginResponse
(
    string TokenKey,
    DateTimeOffset ExpiresAt
);