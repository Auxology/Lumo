using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.SignUp;

internal sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(suc => suc.DisplayName)
            .NotEmpty().WithMessage("Display Name is required")
            .MaximumLength(UserConstants.MaxDisplayNameLength)
            .WithMessage($"Display Name must not exceed {UserConstants.MaxDisplayNameLength} characters");
        
        RuleFor(suc => suc.EmailAddress)
            .NotEmpty().WithMessage("Email Address is required")
            .EmailAddress().WithMessage("Email Address is not valid")
            .MaximumLength(UserConstants.MaxEmailAddressLength)
            .WithMessage($"Email Address must not exceed {UserConstants.MaxEmailAddressLength} characters");
    }
}