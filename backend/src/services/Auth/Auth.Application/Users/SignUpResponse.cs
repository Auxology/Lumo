namespace Auth.Application.Users;

public sealed record SignUpResponse
(
    Guid UserId,
    IReadOnlyList<string> RecoveryCodes
);