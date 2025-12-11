using Auth.Application.Users.FinalizeAvatarUpload;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Users.FinalizeAvatarUpload;

public sealed class FinalizeAvatarUploadCommandValidatorTests
{
    private readonly FinalizeAvatarUploadCommandValidator _validator;

    public FinalizeAvatarUploadCommandValidatorTests()
    {
        _validator = new();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        FinalizeAvatarUploadCommand command = new
        (
            AvatarKey: "users/12345/files/avatar.jpg"
        );

        TestValidationResult<FinalizeAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyAvatarKey_ShouldFail(string? avatarKey)
    {
        FinalizeAvatarUploadCommand command = new
        (
            AvatarKey: avatarKey!
        );

        TestValidationResult<FinalizeAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AvatarKey);
    }
}
