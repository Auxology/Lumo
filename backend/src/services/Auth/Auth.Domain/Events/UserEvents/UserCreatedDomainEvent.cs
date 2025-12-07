using SharedKernel.Domain;

namespace Auth.Domain.Events.UserEvents;

public sealed record UserCreatedDomainEvent
(
    Guid UserId,
    string DisplayName,
    string EmailAddress,
    DateTimeOffset OccurredOn
) : IDomainEvent;