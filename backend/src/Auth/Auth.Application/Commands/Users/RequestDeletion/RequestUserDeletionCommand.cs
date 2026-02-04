using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Users.RequestDeletion;

public sealed record RequestUserDeletionCommand() : ICommand<RequestUserDeletionResponse>;