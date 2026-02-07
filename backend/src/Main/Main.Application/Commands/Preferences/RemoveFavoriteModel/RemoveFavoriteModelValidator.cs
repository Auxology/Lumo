using FluentValidation;

namespace Main.Application.Commands.Preferences.RemoveFavoriteModel;

internal sealed class RemoveFavoriteModelValidator : AbstractValidator<RemoveFavoriteModelCommand>
{
    public RemoveFavoriteModelValidator()
    {
        RuleFor(cmd => cmd.FavoriteModelId)
            .NotEmpty().WithMessage("Favorite Model ID is required");
    }
}