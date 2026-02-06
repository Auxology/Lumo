using FluentValidation;

using Main.Domain.Constants;

namespace Main.Application.Commands.Preferences.AddFavoriteModel;

internal sealed class AddFavoriteModelValidator : AbstractValidator<AddFavoriteModelCommand>
{
    public AddFavoriteModelValidator()
    {
        RuleFor(cmd => cmd.ModelId)
            .NotEmpty().WithMessage("Model ID is required")
            .MaximumLength(FavoriteModelConstants.MaxModelIdLength)
            .WithMessage($"Model ID must not exceed {FavoriteModelConstants.MaxModelIdLength} characters");
    }
}