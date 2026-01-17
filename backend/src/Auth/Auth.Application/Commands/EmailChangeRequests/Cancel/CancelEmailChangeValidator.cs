using FluentValidation;

namespace Auth.Application.Commands.EmailChangeRequests.Cancel;

internal sealed class CancelEmailChangeValidator : AbstractValidator<CancelEmailChangeCommand>
{
    public CancelEmailChangeValidator()
    {
        RuleFor(cecc => cecc.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required.");
    }
}