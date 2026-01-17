using Auth.Domain.Constants;

using FluentValidation;

namespace Auth.Application.Commands.EmailChangeRequests.Request;

internal sealed class RequestEmailChangeValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeValidator()
    {
        RuleFor(recc => recc.NewEmailAddress)
            .NotEmpty()
            .WithMessage("New email address is required.")
            .MaximumLength(UserConstants.MaxEmailAddressLength)
            .WithMessage($"Email address must not exceed {UserConstants.MaxEmailAddressLength} characters.")
            .EmailAddress()
            .WithMessage("Please provide a valid email address.");
    }
}