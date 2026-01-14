namespace Contracts.Requests;

public sealed record LogoutApiRequest
(
    string RefreshToken
);