namespace Contracts.Responses;

public sealed record RefreshSessionApiResponse
(
    string AccessToken,
    string RefreshToken
);