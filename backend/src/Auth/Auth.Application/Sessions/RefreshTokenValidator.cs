using FluentValidation;

namespace Auth.Application.Sessions;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(rtc => rtc.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is required");
    }
}
