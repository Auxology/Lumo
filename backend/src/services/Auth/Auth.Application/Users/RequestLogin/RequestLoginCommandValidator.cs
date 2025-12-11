using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.RequestLogin;

internal sealed class RequestLoginCommandValidator : AbstractValidator<RequestLoginCommand>
{
    public RequestLoginCommandValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .EmailAddress()
            .WithMessage("Email address must be valid.")
            .MaximumLength(UserConstants.MaxEmailLength)
            .WithMessage($"Email address must not exceed {UserConstants.MaxEmailLength} characters.");
    }
}