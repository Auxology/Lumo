namespace Auth.Api.Endpoints.Users.SignUp;

internal sealed record Request
(
    string DisplayName,
    string EmailAddress
);