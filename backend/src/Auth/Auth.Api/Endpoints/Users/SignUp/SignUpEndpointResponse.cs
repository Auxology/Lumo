namespace Auth.Api.Endpoints.Users.SignUp;

internal sealed record SignUpEndpointResponse
(
    IReadOnlyCollection<string> UserFriendlyRecoveryKeys
);