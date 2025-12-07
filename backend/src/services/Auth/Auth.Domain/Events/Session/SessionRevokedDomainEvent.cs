using SharedKernel.Domain;

namespace Auth.Domain.Events.Session;

public sealed record SessionRevokedDomainEvent
(
    Guid SessionId,
    Guid UserId,
    string Reason,
    DateTimeOffset OccurredOn
) : IDomainEvent;