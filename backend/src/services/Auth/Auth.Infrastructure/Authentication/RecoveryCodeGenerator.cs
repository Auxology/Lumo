using System.Security.Cryptography;
using Auth.Application.Abstractions.Authentication;

namespace Auth.Infrastructure.Authentication;

public sealed class RecoveryCodeGenerator : IRecoveryCodeGenerator
{
    private const int MinCodes = 1;
    private const int MaxCodes = 100;
    private const int MinLength = 8;
    private const int MaxLength = 32;
    private const string CharacterSet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public IReadOnlyList<string> GenerateCodes(int count, int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, MinCodes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, MaxCodes);
        ArgumentOutOfRangeException.ThrowIfLessThan(length, MinLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MaxLength);

        HashSet<string> codes = new(count);
        int maxAttempts = count * 10;
        int attempts = 0;

        while (codes.Count < count && attempts++ < maxAttempts)
        {
            codes.Add(GenerateCode(length));
        }

        return [.. codes];
    }

    private static string GenerateCode(int length)
    {
        return string.Create(length, length, static (span, len) =>
        {
            for (int i = 0; i < len; i++)
            {
                span[i] = CharacterSet[RandomNumberGenerator.GetInt32(CharacterSet.Length)];
            }
        });
    }
}   