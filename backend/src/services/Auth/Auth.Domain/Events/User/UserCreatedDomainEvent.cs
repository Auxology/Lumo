using SharedKernel.Domain;

namespace Auth.Domain.Events.User;

public sealed record UserCreatedDomainEvent
(
    Guid UserId,
    string DisplayName,
    string EmailAddress,
    DateTimeOffset OccurredOn
) : IDomainEvent;