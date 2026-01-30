using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class SharedChatIdTests
{
    private const string ValidSharedChatId = "sht_01JGX123456789012345678901";
    private const string Prefix = "sht_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<SharedChatId> outcome = SharedChatId.From(ValidSharedChatId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidSharedChatId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<SharedChatId> outcome = SharedChatId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("SharedChatId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("sht_short")]
    [InlineData("sht_01JGX1234567890123456789012")] // 31 chars - too long
    [InlineData("sht_01JGX1234567890123456789")] // 29 chars - too short
    [InlineData("xxx_01JGX123456789012345678901")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<SharedChatId> outcome = SharedChatId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("SharedChatId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        SharedChatId result = SharedChatId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        SharedChatId id = SharedChatId.UnsafeFrom(ValidSharedChatId);

        string result = id.ToString();

        result.Should().Be(ValidSharedChatId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        SharedChatId id = SharedChatId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        SharedChatId id = SharedChatId.UnsafeFrom(ValidSharedChatId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        SharedChatId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        SharedChatId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        SharedChatId id1 = SharedChatId.UnsafeFrom(ValidSharedChatId);
        SharedChatId id2 = SharedChatId.UnsafeFrom(ValidSharedChatId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        SharedChatId id1 = SharedChatId.UnsafeFrom("sht_01JGX123456789012345678901");
        SharedChatId id2 = SharedChatId.UnsafeFrom("sht_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidSharedChatId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidSharedChatId.Should().StartWith(Prefix);
    }
}