using SharedKernel.Authentication;

namespace Auth.Domain.Tests.TestHelpers;

public sealed class FakeSecretHasher : ISecretHasher
{
    private const string HashSuffix = "_hashed";
    
    public string Hash(string secret) => secret + HashSuffix;
    
    public bool Verify(string secret, string hash) => hash == secret + HashSuffix;
}