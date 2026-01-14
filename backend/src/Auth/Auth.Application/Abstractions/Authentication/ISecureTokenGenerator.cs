namespace Auth.Application.Abstractions.Authentication;

public interface ISecureTokenGenerator
{
    string GenerateToken(int length);

    string HashToken(string token);

    bool VerifyToken(string token, string hashedToken);

    bool TryParseCompoundToken(string compoundToken, out string key, out string token);
}