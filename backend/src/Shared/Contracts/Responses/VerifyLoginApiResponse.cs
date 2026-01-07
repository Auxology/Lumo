namespace Contracts.Responses;

public sealed record VerifyLoginApiResponse
(
    string AccessToken,
    string RefreshToken
);