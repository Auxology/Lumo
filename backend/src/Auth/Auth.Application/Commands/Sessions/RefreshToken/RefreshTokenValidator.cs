using FluentValidation;

namespace Auth.Application.Commands.Sessions.RefreshToken;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(rtc => rtc.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is required");
    }
}