using SharedKernel.Application.Messaging;

namespace Auth.Application.Queries.Sessions.GetActiveSessionCount;

public sealed record GetActiveSessionCountQuery : IQuery<int>;