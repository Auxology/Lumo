using Auth.Application.Abstractions.Authentication;

namespace Auth.Application.Tests.TestHelpers;

internal sealed class FakeRecoveryCodeGenerator : IRecoveryCodeGenerator
{
    public IReadOnlyList<string> GenerateCodes(int count, int length)
    {
        return Enumerable.Range(1, count)
            .Select(i => $"RC{i:D4}".PadRight(length, 'X'))
            .ToList();
    }
}
