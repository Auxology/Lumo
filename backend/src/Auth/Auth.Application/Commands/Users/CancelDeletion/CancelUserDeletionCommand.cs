using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Users.CancelDeletion;

public sealed record CancelUserDeletionCommand() : ICommand<CancelUserDeletionResponse>;