namespace Auth.Application.Users.SignUp;

public sealed record SignUpResponse
(
    IReadOnlyList<string> RecoveryKeys
);