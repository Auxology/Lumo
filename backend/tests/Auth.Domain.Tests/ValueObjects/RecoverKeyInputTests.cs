using Auth.Domain.ValueObjects;

using FluentAssertions;

namespace Auth.Domain.Tests.ValueObjects;

internal sealed class RecoverKeyInputTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        string identifier = "recovery-key-123";
        string verifierHash = "hashed-verifier";

        RecoverKeyInput result = RecoverKeyInput.Create(identifier, verifierHash);

        result.Identifier.Should().Be(identifier);
        result.VerifierHash.Should().Be(verifierHash);
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        RecoverKeyInput input1 = RecoverKeyInput.Create("id", "hash");
        RecoverKeyInput input2 = RecoverKeyInput.Create("id", "hash");

        input1.Should().Be(input2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        RecoverKeyInput input1 = RecoverKeyInput.Create("id1", "hash1");
        RecoverKeyInput input2 = RecoverKeyInput.Create("id2", "hash2");

        input1.Should().NotBe(input2);
    }
}