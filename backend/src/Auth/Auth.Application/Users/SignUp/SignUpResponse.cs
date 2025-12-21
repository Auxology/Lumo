namespace Auth.Application.Users.SignUp;

public sealed record SignUpResponse
(
    IReadOnlyCollection<string> UserFriendlyRecoveryKeys
);