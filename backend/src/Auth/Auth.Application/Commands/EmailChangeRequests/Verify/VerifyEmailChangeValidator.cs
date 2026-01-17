using FluentValidation;

namespace Auth.Application.Commands.EmailChangeRequests.Verify;

internal sealed class VerifyEmailChangeValidator : AbstractValidator<VerifyEmailChangeCommand>
{
    public VerifyEmailChangeValidator()
    {
        RuleFor(vecc => vecc.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required.");

        RuleFor(vecc => vecc.OtpToken)
            .NotEmpty()
            .WithMessage("OTP token is required.");
    }
}