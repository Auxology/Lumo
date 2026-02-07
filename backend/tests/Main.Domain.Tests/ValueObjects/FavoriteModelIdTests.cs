using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class FavoriteModelIdTests
{
    private const string ValidFavoriteModelId = "fmd_01JGX123456789012345678901";
    private const string Prefix = "fmd_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<FavoriteModelId> outcome = FavoriteModelId.From(ValidFavoriteModelId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidFavoriteModelId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<FavoriteModelId> outcome = FavoriteModelId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("FavoriteModelId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("fmd_short")]
    [InlineData("fmd_01JGX123456789012345678")] // wrong length
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<FavoriteModelId> outcome = FavoriteModelId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("FavoriteModelId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        FavoriteModelId result = FavoriteModelId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        FavoriteModelId id = FavoriteModelId.UnsafeFrom(ValidFavoriteModelId);

        string result = id.ToString();

        result.Should().Be(ValidFavoriteModelId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        FavoriteModelId id = FavoriteModelId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        FavoriteModelId id = FavoriteModelId.UnsafeFrom(ValidFavoriteModelId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        FavoriteModelId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        FavoriteModelId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        FavoriteModelId id1 = FavoriteModelId.UnsafeFrom(ValidFavoriteModelId);
        FavoriteModelId id2 = FavoriteModelId.UnsafeFrom(ValidFavoriteModelId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        FavoriteModelId id1 = FavoriteModelId.UnsafeFrom("fmd_01JGX123456789012345678901");
        FavoriteModelId id2 = FavoriteModelId.UnsafeFrom("fmd_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidFavoriteModelId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidFavoriteModelId.Should().StartWith(Prefix);
    }
}