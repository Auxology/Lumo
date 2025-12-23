using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.LoginRequests.Verify;

internal sealed class VerifyLoginValidator : AbstractValidator<VerifyLoginCommand>
{
    public VerifyLoginValidator()
    {
        RuleFor(vlc => vlc.TokenKey)
            .NotEmpty().WithMessage("Token Key is required")
            .Length(LoginRequestConstants.TokenKeyLength)
            .WithMessage($"Token Key must be exactly {LoginRequestConstants.TokenKeyLength} characters");

        RuleFor(vlc => vlc)
            .Must(HaveExactlyOneToken)
            .WithMessage("Exactly one verification method must be provided (OTP or Magic Link)");

        When(vlc => !string.IsNullOrWhiteSpace(vlc.OtpToken), () =>
        {
            RuleFor(vlc => vlc.OtpToken)
                .Length(LoginRequestConstants.OtpTokenLength)
                .WithMessage($"OTP Token must be exactly {LoginRequestConstants.OtpTokenLength} characters");
        });

        When(vlc => !string.IsNullOrWhiteSpace(vlc.MagicLinkToken), () =>
        {
            RuleFor(vlc => vlc.MagicLinkToken)
                .Length(LoginRequestConstants.MagicLinkTokenLength)
                .WithMessage($"Magic Link Token must be exactly {LoginRequestConstants.MagicLinkTokenLength} characters");
        });
    }

    private static bool HaveExactlyOneToken(VerifyLoginCommand command)
    {
        bool hasOtp = !string.IsNullOrWhiteSpace(command.OtpToken);
        bool hasMagicLink = !string.IsNullOrWhiteSpace(command.MagicLinkToken);

        return hasOtp ^ hasMagicLink;
    }
}