using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class LoginRequestIdTests
{
    private const string ValidLoginRequestId = "lrq_01JGX123456789012345678901";
    private const string Prefix = "lrq_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<LoginRequestId> outcome = LoginRequestId.From(ValidLoginRequestId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidLoginRequestId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<LoginRequestId> outcome = LoginRequestId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("LoginRequestId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("lrq_short")]
    [InlineData("lrq_01JGX123456789012345678")] // 29 chars - too short
    [InlineData("lrq_01JGX1234567890123456789")] // 31 chars - too long
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<LoginRequestId> outcome = LoginRequestId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("LoginRequestId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        LoginRequestId result = LoginRequestId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        LoginRequestId id = LoginRequestId.UnsafeFrom(ValidLoginRequestId);

        string result = id.ToString();

        result.Should().Be(ValidLoginRequestId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        LoginRequestId id = LoginRequestId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        LoginRequestId id = LoginRequestId.UnsafeFrom(ValidLoginRequestId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        LoginRequestId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        LoginRequestId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        LoginRequestId id1 = LoginRequestId.UnsafeFrom(ValidLoginRequestId);
        LoginRequestId id2 = LoginRequestId.UnsafeFrom(ValidLoginRequestId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        LoginRequestId id1 = LoginRequestId.UnsafeFrom("lrq_01JGX123456789012345678901");
        LoginRequestId id2 = LoginRequestId.UnsafeFrom("lrq_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidLoginRequestId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidLoginRequestId.Should().StartWith(Prefix);
    }
}