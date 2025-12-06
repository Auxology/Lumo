using SharedKernel.Domain;

namespace Auth.Domain.Events.Session;

public sealed record SessionCreatedDomainEvent
(
    Guid SessionId,
    Guid UserId,
    string IpAddress,
    string UserAgent,
    DateTimeOffset OccurredOn
) : IDomainEvent;