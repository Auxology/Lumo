using FluentValidation;

namespace Auth.Application.Commands.EmailChangeRequests.Verify;

internal sealed class VerifyEmailChangeValidator : AbstractValidator<VerifyEmailChangeCommand>
{
    public VerifyEmailChangeValidator()
    {
        RuleFor(vecc => vecc.TokenKey)
            .NotEmpty()
            .WithMessage("Token key is required.");

        RuleFor(vecc => vecc)
            .Must(vecc => !string.IsNullOrWhiteSpace(vecc.OtpToken) || !string.IsNullOrWhiteSpace(vecc.MagicLinkToken))
            .WithMessage("Either OTP token or magic link token must be provided.");
    }
}