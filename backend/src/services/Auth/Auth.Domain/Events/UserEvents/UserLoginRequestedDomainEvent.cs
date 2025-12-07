using SharedKernel.Domain;

namespace Auth.Domain.Events.UserEvents;

public sealed record UserLoginRequestedDomainEvent
(
    Guid UserId,
    string EmailAddress,
    string OtpToken,
    string MagicLinkToken,
    string IpAddress,
    string UserAgent,
    DateTimeOffset OccurredOn
) : IDomainEvent;