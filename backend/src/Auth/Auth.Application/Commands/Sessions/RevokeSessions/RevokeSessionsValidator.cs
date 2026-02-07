using FluentValidation;

namespace Auth.Application.Commands.Sessions.RevokeSessions;

internal sealed class RevokeSessionsValidator : AbstractValidator<RevokeSessionsCommand>
{
    public RevokeSessionsValidator()
    {
        RuleFor(x => x.SessionIds)
            .NotEmpty().WithMessage("At least one session ID is required.");
    }
}