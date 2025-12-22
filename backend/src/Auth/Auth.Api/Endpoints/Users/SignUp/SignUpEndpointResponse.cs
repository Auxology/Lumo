namespace Auth.Api.Endpoints.Users.SignUp;

public sealed record SignUpEndpointResponse
(
    IReadOnlyCollection<string> UserFriendlyRecoveryKeys
);