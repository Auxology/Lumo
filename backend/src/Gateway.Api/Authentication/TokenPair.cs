namespace Gateway.Api.Authentication;

internal sealed record TokenPair
(
    string AccessToken,
    string RefreshToken
);