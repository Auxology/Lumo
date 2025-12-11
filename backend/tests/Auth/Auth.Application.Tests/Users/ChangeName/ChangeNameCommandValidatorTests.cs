using Auth.Application.Users.ChangeName;
using Auth.Domain.Constants;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Users.ChangeName;

public sealed class ChangeNameCommandValidatorTests
{
    private readonly ChangeNameCommandValidator _validator;

    public ChangeNameCommandValidatorTests()
    {
        _validator = new();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        ChangeNameCommand command = new
        (
            NewDisplayName: "John Doe"
        );

        TestValidationResult<ChangeNameCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyDisplayName_ShouldFail(string? displayName)
    {
        ChangeNameCommand command = new
        (
            NewDisplayName: displayName!
        );

        TestValidationResult<ChangeNameCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewDisplayName);
    }

    [Fact]
    public void Validate_WithTooLongDisplayName_ShouldFail()
    {
        string displayName = new('a', UserConstants.MaxDisplayNameLength + 1);

        ChangeNameCommand command = new
        (
            NewDisplayName: displayName
        );

        TestValidationResult<ChangeNameCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewDisplayName);
    }
}
