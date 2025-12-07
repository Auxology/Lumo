using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users;

internal sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(UserConstants.MaxDisplayNameLength)
            .WithMessage($"Display name must not exceed {UserConstants.MaxDisplayNameLength} characters.");

        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .EmailAddress()
            .WithMessage("Email address must be valid.")
            .MaximumLength(UserConstants.MaxEmailLength)
            .WithMessage($"Email address must not exceed {UserConstants.MaxEmailLength} characters.");
    }
}