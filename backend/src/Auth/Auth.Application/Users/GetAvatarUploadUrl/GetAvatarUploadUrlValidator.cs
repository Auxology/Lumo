using Auth.Application.Abstractions.Storage;

using FluentValidation;

namespace Auth.Application.Users.GetAvatarUploadUrl;

internal sealed class GetAvatarUploadUrlValidator : AbstractValidator<GetAvatarUploadUrlCommand>
{
    public GetAvatarUploadUrlValidator()
    {
        RuleFor(gauuc => gauuc.ContentType)
            .NotEmpty().WithMessage("Content Type is required")
            .Must(ct => AvatarConstants.AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Content Type must be one of: {string.Join(", ", AvatarConstants.AllowedContentTypes)}");

        RuleFor(gauuc => gauuc.ContentLength)
            .GreaterThan(0).WithMessage("Content Length must be greater than 0")
            .LessThanOrEqualTo(AvatarConstants.MaxFileSizeInBytes)
            .WithMessage($"Content Length must not exceed {AvatarConstants.MaxFileSizeInBytes / (1024 * 1024)} MB");
    }
}