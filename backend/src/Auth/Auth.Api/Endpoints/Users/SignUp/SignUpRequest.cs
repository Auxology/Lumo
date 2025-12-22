namespace Auth.Api.Endpoints.Users.SignUp;

public sealed record SignUpRequest
(
    string DisplayName,
    string EmailAddress
);