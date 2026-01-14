using Auth.Domain.Constants;

using FluentValidation;

namespace Auth.Application.Commands.RecoveryRequests.VerifyNewEmail;

internal sealed class VerifyNewEmailValidator : AbstractValidator<VerifyNewEmailCommand>
{
    public VerifyNewEmailValidator()
    {
        RuleFor(vnec => vnec.TokenKey)
            .NotEmpty().WithMessage("Token key is required.")
            .Length(RecoveryRequestConstants.TokenKeyLength)
            .WithMessage($"Token key must be exactly {RecoveryRequestConstants.TokenKeyLength} characters.");

        RuleFor(vnec => vnec)
            .Must(HaveExactlyOneToken)
            .WithMessage("Exactly one verification method must be provided (OTP or Magic Link).");

        When(vnec => !string.IsNullOrWhiteSpace(vnec.OtpToken), () =>
        {
            RuleFor(vnec => vnec.OtpToken)
                .Length(RecoveryRequestConstants.OtpTokenLength)
                .WithMessage($"OTP token must be exactly {RecoveryRequestConstants.OtpTokenLength} characters.");
        });

        When(vnec => !string.IsNullOrWhiteSpace(vnec.MagicLinkToken), () =>
        {
            RuleFor(vnec => vnec.MagicLinkToken)
                .Length(RecoveryRequestConstants.MagicLinkTokenLength)
                .WithMessage($"Magic link token must be exactly {RecoveryRequestConstants.MagicLinkTokenLength} characters.");
        });
    }

    private static bool HaveExactlyOneToken(VerifyNewEmailCommand command)
    {
        bool hasOtp = !string.IsNullOrWhiteSpace(command.OtpToken);
        bool hasMagicLink = !string.IsNullOrWhiteSpace(command.MagicLinkToken);
        return hasOtp ^ hasMagicLink;
    }
}