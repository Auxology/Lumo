using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class InstructionIdTests
{
    private const string ValidInstructionId = "ins_01JGX123456789012345678901";
    private const string Prefix = "ins_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<InstructionId> outcome = InstructionId.From(ValidInstructionId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidInstructionId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<InstructionId> outcome = InstructionId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("InstructionId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("ins_short")]
    [InlineData("ins_01JGX1234567890123456789012")] // 31 chars - too long
    [InlineData("ins_01JGX1234567890123456789")] // 29 chars - too short
    [InlineData("xxx_01JGX123456789012345678901")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<InstructionId> outcome = InstructionId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("InstructionId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        InstructionId result = InstructionId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        InstructionId id = InstructionId.UnsafeFrom(ValidInstructionId);

        string result = id.ToString();

        result.Should().Be(ValidInstructionId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        InstructionId id = InstructionId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        InstructionId id = InstructionId.UnsafeFrom(ValidInstructionId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        InstructionId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        InstructionId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        InstructionId id1 = InstructionId.UnsafeFrom(ValidInstructionId);
        InstructionId id2 = InstructionId.UnsafeFrom(ValidInstructionId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        InstructionId id1 = InstructionId.UnsafeFrom("ins_01JGX123456789012345678901");
        InstructionId id2 = InstructionId.UnsafeFrom("ins_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidInstructionId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidInstructionId.Should().StartWith(Prefix);
    }
}