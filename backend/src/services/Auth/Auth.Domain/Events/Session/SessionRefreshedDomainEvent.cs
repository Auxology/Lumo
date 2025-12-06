using SharedKernel.Domain;

namespace Auth.Domain.Events.Session;

public sealed record SessionRefreshedDomainEvent
(
    Guid SessionId,
    Guid UserId,
    int NewVersion,
    DateTimeOffset OccurredOn,
    DateTimeOffset NewExpiresAt
) : IDomainEvent;