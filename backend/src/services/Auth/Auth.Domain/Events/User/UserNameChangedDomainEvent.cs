using SharedKernel.Domain;

namespace Auth.Domain.Events.User;

public sealed record UserNameChangedDomainEvent
(
    Guid UserId,
    string NewDisplayName,
    string EmailAddress,
    DateTimeOffset OccurredOn
) : IDomainEvent;