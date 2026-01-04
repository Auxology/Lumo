using System.Security.Cryptography;
using System.Text;

using Auth.Application.Abstractions.Authentication;

namespace Auth.Infrastructure.Authentication;

internal sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public string GenerateToken(int length)
    {
        const int maxUnbiased = 256 - (256 % 62);
        char[] result = new char[length];
        int position = 0;

        while (position < length)
        {
            byte b = RandomNumberGenerator.GetBytes(1)[0];
            if (b < maxUnbiased)
                result[position++] = AllowedCharacters[b % 62];
        }

        return new string(result);
    }

    public string HashToken(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyToken(string token, string hashedToken)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hashedToken))
            return false;

        try
        {
            byte[] computedHashBytes = Convert.FromBase64String(HashToken(token));
            byte[] storedHashBytes = Convert.FromBase64String(hashedToken);
            return CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}