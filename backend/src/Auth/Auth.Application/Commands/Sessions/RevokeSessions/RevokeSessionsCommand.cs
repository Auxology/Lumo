using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Sessions.RevokeSessions;

public sealed record RevokeSessionsCommand(IReadOnlyList<string> SessionIds) : ICommand;