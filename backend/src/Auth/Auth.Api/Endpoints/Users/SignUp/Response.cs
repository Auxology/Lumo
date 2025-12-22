namespace Auth.Api.Endpoints.Users.SignUp;

internal sealed record Response
(
    IReadOnlyCollection<string> UserFriendlyRecoveryKeys
);