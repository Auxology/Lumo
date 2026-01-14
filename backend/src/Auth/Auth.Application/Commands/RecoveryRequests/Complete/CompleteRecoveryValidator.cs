using Auth.Domain.Constants;

using FluentValidation;

namespace Auth.Application.Commands.RecoveryRequests.Complete;


internal sealed class CompleteRecoveryValidator : AbstractValidator<CompleteRecoveryCommand>
{
    public CompleteRecoveryValidator()
    {
        RuleFor(cr => cr.TokenKey)
            .NotEmpty().WithMessage("Token key is required.")
            .Length(RecoveryRequestConstants.TokenKeyLength)
            .WithMessage($"Token key must be exactly {RecoveryRequestConstants.TokenKeyLength} characters.");
    }
}