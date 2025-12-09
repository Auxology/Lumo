using FluentValidation;

namespace Auth.Application.Users.FinalizeAvatarUpload;

internal sealed class FinalizeAvatarUploadCommandValidator : AbstractValidator<FinalizeAvatarUploadCommand>
{
    public FinalizeAvatarUploadCommandValidator()
    {
        RuleFor(x => x.AvatarKey)
            .NotEmpty()
            .WithMessage("Avatar key is required.");
    }
}
