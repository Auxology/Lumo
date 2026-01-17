using FluentValidation;

namespace Auth.Application.Commands.Sessions.Logout;

internal sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(rtc => rtc.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is required");
    }
}