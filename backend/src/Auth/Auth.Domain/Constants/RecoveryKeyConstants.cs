namespace Auth.Domain.Constants;

public static class RecoveryKeyConstants
{
    public const int MaxKeysPerChain = 10;
    
    public const int IdentifierByteLength = 16;
    
    public const int VerifierByteLength = 32;
}