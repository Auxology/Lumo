using SharedKernel.Application.Messaging;

namespace Auth.Application.Users;

public sealed record SignUpCommand
(
    string DisplayName,
    string EmailAddress
) : ICommand<SignUpResponse>;