using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class RecoveryKeyChainIdTests
{
    private const string ValidRecoveryKeyChainId = "rkc_01JGX123456789012345678901";
    private const string Prefix = "rkc_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<RecoveryKeyChainId> outcome = RecoveryKeyChainId.From(ValidRecoveryKeyChainId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidRecoveryKeyChainId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<RecoveryKeyChainId> outcome = RecoveryKeyChainId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("RecoveryKeyChainId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("rkc_short")]
    [InlineData("rkc_01JGX123456789012345678")] // 29 chars - too short
    [InlineData("rkc_01JGX1234567890123456789")] // 31 chars - too long
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<RecoveryKeyChainId> outcome = RecoveryKeyChainId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("RecoveryKeyChainId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        RecoveryKeyChainId result = RecoveryKeyChainId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        RecoveryKeyChainId id = RecoveryKeyChainId.UnsafeFrom(ValidRecoveryKeyChainId);

        string result = id.ToString();

        result.Should().Be(ValidRecoveryKeyChainId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        RecoveryKeyChainId id = RecoveryKeyChainId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        RecoveryKeyChainId id = RecoveryKeyChainId.UnsafeFrom(ValidRecoveryKeyChainId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        RecoveryKeyChainId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        RecoveryKeyChainId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        RecoveryKeyChainId id1 = RecoveryKeyChainId.UnsafeFrom(ValidRecoveryKeyChainId);
        RecoveryKeyChainId id2 = RecoveryKeyChainId.UnsafeFrom(ValidRecoveryKeyChainId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        RecoveryKeyChainId id1 = RecoveryKeyChainId.UnsafeFrom("rkc_01JGX123456789012345678901");
        RecoveryKeyChainId id2 = RecoveryKeyChainId.UnsafeFrom("rkc_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidRecoveryKeyChainId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidRecoveryKeyChainId.Should().StartWith(Prefix);
    }
}