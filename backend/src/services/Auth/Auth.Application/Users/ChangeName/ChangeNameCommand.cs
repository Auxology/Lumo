using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.ChangeName;

public sealed record ChangeNameCommand
(
    string NewDisplayName
) : ICommand;