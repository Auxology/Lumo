namespace Auth.Domain.Constants;

public static class RecoveryKeyConstants
{
    public const int MaxKeysPerChain = 10;
    
    public const int RecoveryKeyIdentifierLength = 16;
    
    public const int RecoveryKeyVerifierLength = 32;
}