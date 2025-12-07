namespace Auth.Domain.Constants;

public static class RecoveryCodeConstants
{
    public const int CodeLength = 16;
    
    public const int CodesPerUser = 10;
    
    public const int MaxUnusedCodesPerUser = 10;
}