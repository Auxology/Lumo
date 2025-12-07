namespace Auth.Application.Abstractions.Authentication;

public interface IRecoveryCodeGenerator
{
    IReadOnlyList<string> GenerateCodes(int count, int length);
}