using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.UpdateProfile;

internal sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(upc => upc)
            .Must(upc => upc.NewDisplayName is not null || upc.NewAvatarKey is not null)
            .WithMessage("At least one field must be provided to update profile");

        When(upc => upc.NewDisplayName is not null, () =>
        {
            RuleFor(upc => upc.NewDisplayName)
                .NotEmpty().WithMessage("Display Name cannot be empty")
                .MaximumLength(UserConstants.MaxDisplayNameLength)
                .WithMessage($"Display Name must not exceed {UserConstants.MaxDisplayNameLength} characters");
        });

        When(upc => upc.NewAvatarKey is not null, () =>
        {
            RuleFor(upc => upc.NewAvatarKey)
                .NotEmpty().WithMessage("Avatar Key cannot be empty");
        });
    }
}

