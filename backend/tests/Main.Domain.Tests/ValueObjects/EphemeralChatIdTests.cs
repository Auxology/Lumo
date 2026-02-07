using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class EphemeralChatIdTests
{
    private const string ValidEphemeralChatId = "ech_01JGX123456789012345678901";
    private const string Prefix = "ech_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<EphemeralChatId> outcome = EphemeralChatId.From(ValidEphemeralChatId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidEphemeralChatId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<EphemeralChatId> outcome = EphemeralChatId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EphemeralChatId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("ech_short")]
    [InlineData("ech_01JGX123456789012345678")] // wrong length
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<EphemeralChatId> outcome = EphemeralChatId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EphemeralChatId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        EphemeralChatId result = EphemeralChatId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        EphemeralChatId id = EphemeralChatId.UnsafeFrom(ValidEphemeralChatId);

        string result = id.ToString();

        result.Should().Be(ValidEphemeralChatId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        EphemeralChatId id = EphemeralChatId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        EphemeralChatId id = EphemeralChatId.UnsafeFrom(ValidEphemeralChatId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        EphemeralChatId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        EphemeralChatId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        EphemeralChatId id1 = EphemeralChatId.UnsafeFrom(ValidEphemeralChatId);
        EphemeralChatId id2 = EphemeralChatId.UnsafeFrom(ValidEphemeralChatId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        EphemeralChatId id1 = EphemeralChatId.UnsafeFrom("ech_01JGX123456789012345678901");
        EphemeralChatId id2 = EphemeralChatId.UnsafeFrom("ech_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidEphemeralChatId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidEphemeralChatId.Should().StartWith(Prefix);
    }
}