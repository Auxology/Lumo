namespace Contracts.Requests;

public sealed record RefreshSessionApiRequest
(
    string RefreshToken
);