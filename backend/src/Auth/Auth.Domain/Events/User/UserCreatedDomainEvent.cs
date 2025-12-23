using SharedKernel;

namespace Auth.Domain.Events.User;

public sealed record UserCreatedDomainEvent
(
    Guid UserId,
    string EmailAddress,
    DateTimeOffset CreatedAt
) : IDomainEvent;