using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.VerifyLogin;

internal sealed class VerifyLoginCommandValidator : AbstractValidator<VerifyLoginCommand>
{
    public VerifyLoginCommandValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .EmailAddress()
            .WithMessage("Email address must be valid.")
            .MaximumLength(UserConstants.MaxEmailLength)
            .WithMessage($"Email address must not exceed {UserConstants.MaxEmailLength} characters.");
        
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.OtpToken) ||
                       !string.IsNullOrWhiteSpace(x.MagicLinkToken))
            .WithMessage("Either OtpToken or MagicLinkToken must be provided.");

        When(x => !string.IsNullOrWhiteSpace(x.OtpToken), () =>
        {
            RuleFor(x => x.OtpToken)
                .Length(UserTokenConstants.OtpTokenLength)
                .WithMessage($"OTP token must be exactly {UserTokenConstants.OtpTokenLength} characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.MagicLinkToken), () =>
        {
            RuleFor(x => x.MagicLinkToken)
                .Length(UserTokenConstants.MagicLinkTokenLength)
                .WithMessage($"Magic link token must be exactly {UserTokenConstants.MagicLinkTokenLength} characters.");
        });
    }
}