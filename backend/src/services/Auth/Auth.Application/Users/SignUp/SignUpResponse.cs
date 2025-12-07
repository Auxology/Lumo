namespace Auth.Application.Users.SignUp;

public sealed record SignUpResponse
(
    Guid UserId,
    IReadOnlyList<string> RecoveryCodes
);