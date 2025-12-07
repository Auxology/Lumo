using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.ChangeName;

internal sealed class ChangeNameCommandValidator : AbstractValidator<ChangeNameCommand>
{
    public ChangeNameCommandValidator()
    {
        RuleFor(x => x.NewDisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(UserConstants.MaxDisplayNameLength)
            .WithMessage($"Display name must not exceed {UserConstants.MaxDisplayNameLength} characters.");
    }
}
