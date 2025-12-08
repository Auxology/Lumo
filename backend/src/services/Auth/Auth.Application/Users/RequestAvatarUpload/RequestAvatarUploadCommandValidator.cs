using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.RequestAvatarUpload;

internal sealed class RequestAvatarUploadCommandValidator : AbstractValidator<RequestAvatarUploadCommand>
{
    public RequestAvatarUploadCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.")
            .Must(ct => UserConstants.AllowedContentTypes.Contains(ct))
            .WithMessage($"Content type must be one of: {string.Join(", ", UserConstants.AllowedContentTypes)}");

        RuleFor(x => x.ContentLength)
            .GreaterThan(0)
            .WithMessage("Content length must be greater than 0.")
            .LessThanOrEqualTo(UserConstants.MaxAvatarSizeInBytes)
            .WithMessage($"File size must not exceed {UserConstants.MaxAvatarSizeInBytes / (1024 * 1024)} MB.");
    }
}
