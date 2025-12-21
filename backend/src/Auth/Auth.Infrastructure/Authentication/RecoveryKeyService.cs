using System.Security.Cryptography;
using System.Text;
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Models;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;

namespace Auth.Infrastructure.Authentication;

internal sealed class RecoveryKeyService : IRecoveryKeyService
{
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static string GenerateRandomString(int length)
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

    private static string HashVerifier(string verifier)
    {
        byte[] verifierBytes = Encoding.UTF8.GetBytes(verifier);
        
        byte[] hashBytes = SHA256.HashData(verifierBytes);
        
        return Convert.ToBase64String(hashBytes);
    }
    
    public GeneratedRecoveryKeys GenerateRecoveryKeys()
    {
        List<RecoverKeyInput> recoveryKeyInputs = new(RecoveryKeyConstants.MaxKeysPerChain);
        List<string> userFriendlyRecoveryKeys = new(RecoveryKeyConstants.MaxKeysPerChain);

        for (int i = 0; i < RecoveryKeyConstants.MaxKeysPerChain; i++)
        {
            string identifier = GenerateRandomString(RecoveryKeyConstants.IdentifierByteLength);
            string verifier = GenerateRandomString(RecoveryKeyConstants.VerifierByteLength);
            string verifierHash = HashVerifier(verifier);
            string userFriendlyRecoveryKey = $"{identifier}.{verifier}";

            RecoverKeyInput recoverKeyInput = RecoverKeyInput.Create
            (
                identifier: identifier,
                verifierHash: verifierHash
            );
            
            recoveryKeyInputs.Add(recoverKeyInput);
            userFriendlyRecoveryKeys.Add(userFriendlyRecoveryKey);
        }
        
        GeneratedRecoveryKeys generatedRecoveryKeys = new
        (
            RecoverKeyInputs: recoveryKeyInputs,
            UserFriendlyRecoveryKeys: userFriendlyRecoveryKeys
        );
        
        return generatedRecoveryKeys;
    }

    public bool VerifyRecoveryKey(string verifier, string hashedVerifier)
    {
        if (string.IsNullOrEmpty(verifier) || string.IsNullOrEmpty(hashedVerifier))
            return false;
        
        try
        {
            byte[] computedHashBytes = Convert.FromBase64String(HashVerifier(verifier));
            byte[] storedHashBytes = Convert.FromBase64String(hashedVerifier);
            return CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}