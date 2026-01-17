using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class EmailChangeRequestIdTests
{
    private const string ValidEmailChangeRequestId = "ecr_01JGX123456789012345678901";
    private const string Prefix = "ecr_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<EmailChangeRequestId> outcome = EmailChangeRequestId.From(ValidEmailChangeRequestId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidEmailChangeRequestId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<EmailChangeRequestId> outcome = EmailChangeRequestId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EmailChangeRequestId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("ecr_short")]
    [InlineData("ecr_01JGX12345678901234567890")] // 29 chars - too short
    [InlineData("ecr_01JGX1234567890123456789012")] // 31 chars - too long
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<EmailChangeRequestId> outcome = EmailChangeRequestId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EmailChangeRequestId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        EmailChangeRequestId result = EmailChangeRequestId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        EmailChangeRequestId id = EmailChangeRequestId.UnsafeFrom(ValidEmailChangeRequestId);

        string result = id.ToString();

        result.Should().Be(ValidEmailChangeRequestId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        EmailChangeRequestId id = EmailChangeRequestId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        EmailChangeRequestId id = EmailChangeRequestId.UnsafeFrom(ValidEmailChangeRequestId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        EmailChangeRequestId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        EmailChangeRequestId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        EmailChangeRequestId id1 = EmailChangeRequestId.UnsafeFrom(ValidEmailChangeRequestId);
        EmailChangeRequestId id2 = EmailChangeRequestId.UnsafeFrom(ValidEmailChangeRequestId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        EmailChangeRequestId id1 = EmailChangeRequestId.UnsafeFrom("ecr_01JGX123456789012345678901");
        EmailChangeRequestId id2 = EmailChangeRequestId.UnsafeFrom("ecr_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidEmailChangeRequestId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidEmailChangeRequestId.Should().StartWith(Prefix);
    }
}