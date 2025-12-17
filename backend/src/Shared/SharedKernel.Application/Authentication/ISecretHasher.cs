namespace SharedKernel.Application.Authentication;

public interface ISecretHasher
{
    string Hash(string secret);

    bool Verify(string secret, string hashedSecret);
}