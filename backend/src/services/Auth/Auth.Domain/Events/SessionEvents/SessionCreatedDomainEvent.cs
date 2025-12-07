using SharedKernel.Domain;

namespace Auth.Domain.Events.SessionEvents;

public sealed record SessionCreatedDomainEvent
(
    Guid SessionId,
    Guid UserId,
    string IpAddress,
    string UserAgent,
    DateTimeOffset OccurredOn
) : IDomainEvent;