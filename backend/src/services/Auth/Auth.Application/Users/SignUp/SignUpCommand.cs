using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.SignUp;

public sealed record SignUpCommand
(
    string DisplayName,
    string EmailAddress
) : ICommand<SignUpResponse>;