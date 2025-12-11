using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.RequestLogin;

public sealed record RequestLoginCommand
(
    string EmailAddress
) : ICommand;