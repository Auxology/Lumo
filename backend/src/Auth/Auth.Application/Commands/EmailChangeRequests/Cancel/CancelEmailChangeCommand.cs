using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.EmailChangeRequests.Cancel;

public sealed record CancelEmailChangeCommand(string TokenKey) : ICommand;