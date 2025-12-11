using Auth.Application.Users.RequestAvatarUpload;
using Auth.Domain.Constants;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Users.RequestAvatarUpload;

public sealed class RequestAvatarUploadCommandValidatorTests
{
    private readonly RequestAvatarUploadCommandValidator _validator;

    public RequestAvatarUploadCommandValidatorTests()
    {
        _validator = new();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/jpeg",
            ContentLength: 1024 * 1024
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyContentType_ShouldFail(string? contentType)
    {
        RequestAvatarUploadCommand command = new
        (
            ContentType: contentType!,
            ContentLength: 1024 * 1024
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void Validate_WithInvalidContentType_ShouldFail(string contentType)
    {
        RequestAvatarUploadCommand command = new
        (
            ContentType: contentType,
            ContentLength: 1024 * 1024
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Validate_WithAllowedContentTypes_ShouldPass(string contentType)
    {
        RequestAvatarUploadCommand command = new
        (
            ContentType: contentType,
            ContentLength: 1024 * 1024
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithZeroOrNegativeContentLength_ShouldFail(long contentLength)
    {
        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/jpeg",
            ContentLength: contentLength
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContentLength);
    }

    [Fact]
    public void Validate_WithTooLargeContentLength_ShouldFail()
    {
        long contentLength = UserConstants.MaxAvatarSizeInBytes + 1;

        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/jpeg",
            ContentLength: contentLength
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContentLength);
    }

    [Fact]
    public void Validate_WithMaxAllowedContentLength_ShouldPass()
    {
        long contentLength = UserConstants.MaxAvatarSizeInBytes;

        RequestAvatarUploadCommand command = new
        (
            ContentType: "image/jpeg",
            ContentLength: contentLength
        );

        TestValidationResult<RequestAvatarUploadCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
