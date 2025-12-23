namespace Auth.Api.Endpoints.Users.SignUp;

internal sealed record SignUpRequest
(
    string DisplayName,
    string EmailAddress
);