using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using SharedKernel.Authentication;
using SharedKernel.Domain;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Domain.Entities;

public sealed class UserRecoveryCode : Entity<int>
{
    public UserId UserId { get; private set; }
    
    public string RecoveryCodeHash { get; private set; } = null!;
    
    public bool IsRevoked { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UsedAt { get; private set; }
    
    private UserRecoveryCode() { } // For EF Core

    [SetsRequiredMembers]
    private UserRecoveryCode
    (
        UserId userId,
        string recoveryCodeHash,
        DateTimeOffset utcNow
    )
    {
        Id = 0;
        UserId = userId;
        RecoveryCodeHash = recoveryCodeHash;
        IsRevoked = false;
        CreatedAt = utcNow;
        UsedAt = null;
    }

    internal bool IsUsed => UsedAt is not null;
    
    internal bool IsValid => !IsUsed && !IsRevoked;

    internal static Result<UserRecoveryCode> Create
    (
        UserId userId,
        string recoveryCode,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider
    )
    {
        if (userId.IsEmpty())
            return RecoveryCodeErrors.UserIdRequired;
        
        if (string.IsNullOrWhiteSpace(recoveryCode))
            return RecoveryCodeErrors.CodeRequired;
        
        if (recoveryCode.Length > RecoveryCodeConstants.CodeLength)
            return RecoveryCodeErrors.InvalidCodeLength;
        
        string codeHash = secretHasher.Hash(recoveryCode);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        UserRecoveryCode userRecoveryCode = new
        (
            userId: userId,
            recoveryCodeHash: codeHash,
            utcNow: utcNow
        );
        
        return userRecoveryCode;
    }

    internal Result Use
    (
        string recoveryCode,
        ISecretHasher secretHasher,
        IDateTimeProvider dateTimeProvider
    )
    {
        if (IsUsed)
            return RecoveryCodeErrors.AlreadyUsed;

        if (IsRevoked)
            return RecoveryCodeErrors.Revoked;

        if (string.IsNullOrWhiteSpace(recoveryCode))
            return RecoveryCodeErrors.CodeRequired;

        bool isValid = secretHasher.Verify(recoveryCode, RecoveryCodeHash);
        
        if (!isValid)
            return RecoveryCodeErrors.InvalidCode;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        UsedAt = utcNow;
        
        return Result.Success();
    }
    
    internal void Revoke()
    {
        IsRevoked = true;
    }
}