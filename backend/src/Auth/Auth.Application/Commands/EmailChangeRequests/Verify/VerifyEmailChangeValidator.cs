using FluentValidation;

namespace Auth.Application.Commands.EmailChangeRequests.Verify;

internal sealed class VerifyEmailChangeValidator : AbstractValidator<VerifyEmailChangeCommand>
{
    public VerifyEmailChangeValidator()
    {
        RuleFor(vecc => vecc.TokenKey)
            .NotEmpty()
            .WithMessage("Token key is required.");

        RuleFor(vecc => vecc.OtpToken)
            .NotEmpty()
            .WithMessage("OTP token is required.");
    }
}