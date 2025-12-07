using SharedKernel.Domain;

namespace Auth.Domain.Events.SessionEvents;

public sealed record SessionRefreshedDomainEvent
(
    Guid SessionId,
    Guid UserId,
    int NewVersion,
    DateTimeOffset OccurredOn,
    DateTimeOffset NewExpiresAt
) : IDomainEvent;