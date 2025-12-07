using SharedKernel.Domain;

namespace Auth.Domain.Events.User;

public sealed record UserLoginVerifiedDomainEvent
(
    Guid UserId,
    string EmailAddress,
    string IpAddress,
    string UserAgent,
    string VerificationMethod,
    DateTimeOffset OccurredOn
) : IDomainEvent;