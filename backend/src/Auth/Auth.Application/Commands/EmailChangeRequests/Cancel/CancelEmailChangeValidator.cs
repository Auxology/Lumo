using Auth.Application.Commands.EmailChangeRequests.Verify;

using FluentValidation;

namespace Auth.Application.Commands.EmailChangeRequests.Cancel;

internal sealed class CancelEmailChangeValidator : AbstractValidator<CancelEmailChangeCommand>
{
    public CancelEmailChangeValidator()
    {
        RuleFor(cecc => cecc.TokenKey)
            .NotEmpty()
            .WithMessage("Token key is required.");
    }
}