using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class PreferenceIdTests
{
    private const string ValidPreferenceId = "prf_01JGX123456789012345678901";
    private const string Prefix = "prf_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<PreferenceId> outcome = PreferenceId.From(ValidPreferenceId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidPreferenceId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<PreferenceId> outcome = PreferenceId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("PreferenceId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("prf_short")]
    [InlineData("prf_01JGX1234567890123456789012")] // 31 chars - too long
    [InlineData("prf_01JGX1234567890123456789")] // 29 chars - too short
    [InlineData("xxx_01JGX123456789012345678901")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<PreferenceId> outcome = PreferenceId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("PreferenceId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        PreferenceId result = PreferenceId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        PreferenceId id = PreferenceId.UnsafeFrom(ValidPreferenceId);

        string result = id.ToString();

        result.Should().Be(ValidPreferenceId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        PreferenceId id = PreferenceId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        PreferenceId id = PreferenceId.UnsafeFrom(ValidPreferenceId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        PreferenceId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        PreferenceId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        PreferenceId id1 = PreferenceId.UnsafeFrom(ValidPreferenceId);
        PreferenceId id2 = PreferenceId.UnsafeFrom(ValidPreferenceId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        PreferenceId id1 = PreferenceId.UnsafeFrom("prf_01JGX123456789012345678901");
        PreferenceId id2 = PreferenceId.UnsafeFrom("prf_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidPreferenceId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidPreferenceId.Should().StartWith(Prefix);
    }
}