namespace Auth.Application.Commands.Users.SignUp;

public sealed record SignUpResponse
(
    IReadOnlyCollection<string> UserFriendlyRecoveryKeys
);