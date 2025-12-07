namespace SharedKernel.Authentication;

public interface ISecretHasher
{
    string Hash(string secret);
    
    bool Verify(string secret, string hash);
}