using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.EmailChangeRequests.Cancel;

public sealed record CancelEmailChangeCommand(string RequestId) : ICommand;