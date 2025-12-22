using Auth.Application.Models;

namespace Auth.Application.Abstractions.Authentication;

public interface IRecoveryKeyService
{
    GeneratedRecoveryKeys GenerateRecoveryKeys();

    bool VerifyRecoveryKey(string verifier, string hashedVerifier);
}



