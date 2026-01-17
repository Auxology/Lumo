using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class RecoveryRequestIdTests
{
    private const string ValidRecoveryRequestId = "rr_01JGX123456789012345678901";
    private const string Prefix = "rr_";
    private const int ExpectedLength = 29;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<RecoveryRequestId> outcome = RecoveryRequestId.From(ValidRecoveryRequestId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidRecoveryRequestId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<RecoveryRequestId> outcome = RecoveryRequestId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("RecoveryRequestId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("rr_short")]
    [InlineData("rr_01JGX12345678901234567890")] // 28 chars - too short
    [InlineData("rr_01JGX1234567890123456789012")] // 30 chars - too long
    [InlineData("xx_01JGX123456789012345678901")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<RecoveryRequestId> outcome = RecoveryRequestId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("RecoveryRequestId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        RecoveryRequestId result = RecoveryRequestId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        RecoveryRequestId id = RecoveryRequestId.UnsafeFrom(ValidRecoveryRequestId);

        string result = id.ToString();

        result.Should().Be(ValidRecoveryRequestId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        RecoveryRequestId id = RecoveryRequestId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        RecoveryRequestId id = RecoveryRequestId.UnsafeFrom(ValidRecoveryRequestId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        RecoveryRequestId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        RecoveryRequestId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        RecoveryRequestId id1 = RecoveryRequestId.UnsafeFrom(ValidRecoveryRequestId);
        RecoveryRequestId id2 = RecoveryRequestId.UnsafeFrom(ValidRecoveryRequestId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        RecoveryRequestId id1 = RecoveryRequestId.UnsafeFrom("rr_01JGX123456789012345678901");
        RecoveryRequestId id2 = RecoveryRequestId.UnsafeFrom("rr_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidRecoveryRequestId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidRecoveryRequestId.Should().StartWith(Prefix);
    }
}