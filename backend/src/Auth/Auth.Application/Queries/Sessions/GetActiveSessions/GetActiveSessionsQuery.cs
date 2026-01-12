using SharedKernel.Application.Messaging;

namespace Auth.Application.Queries.Sessions.GetActiveSessions;

public class GetActiveSessionsQuery : IQuery<IReadOnlyList<ActiveSessionReadModel>>;