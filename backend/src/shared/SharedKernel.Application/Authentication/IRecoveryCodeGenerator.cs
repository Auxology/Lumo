namespace SharedKernel.Application.Authentication;

public interface IRecoveryCodeGenerator
{
    IReadOnlyList<string> GenerateCodes(int count, int length);
}