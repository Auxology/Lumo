using SharedKernel.Domain;

namespace Auth.Domain.Events.UserEvents;

public sealed record UserNameChangedDomainEvent
(
    Guid UserId,
    string NewDisplayName,
    string EmailAddress,
    DateTimeOffset OccurredOn
) : IDomainEvent;