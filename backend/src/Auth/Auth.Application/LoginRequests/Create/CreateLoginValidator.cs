using Auth.Domain.Constants;

using FluentValidation;

namespace Auth.Application.LoginRequests.Create;

internal sealed class CreateLoginValidator : AbstractValidator<CreateLoginCommand>
{
    public CreateLoginValidator()
    {
        RuleFor(clc => clc.EmailAddress)
            .NotEmpty().WithMessage("Email Address is required")
            .EmailAddress().WithMessage("Email Address is not valid")
            .MaximumLength(UserConstants.MaxEmailAddressLength)
            .WithMessage($"Email Address must not exceed {UserConstants.MaxEmailAddressLength} characters");
    }
}