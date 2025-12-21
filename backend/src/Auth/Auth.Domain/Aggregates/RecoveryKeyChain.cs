using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;
using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class RecoveryKeyChain : AggregateRoot<RecoveryKeyChainId>
{
    private readonly List<RecoveryKey> _recoveryKeys = [];
    
    public UserId UserId { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? LastRotatedAt { get; private set; }
    
    public int Version { get; private set; }
    
    public IReadOnlyCollection<RecoveryKey> RecoveryKeys => [.._recoveryKeys];
    
    private RecoveryKeyChain() {} // For EF Core
    
    [SetsRequiredMembers]
    private RecoveryKeyChain
    (
        RecoveryKeyChainId id,
        UserId userId,
        DateTimeOffset utcNow,
        List<RecoveryKey> recoveryKeys
    )
    {
        Id = id;
        UserId = userId;
        CreatedAt = utcNow;
        LastRotatedAt = null;
        Version = 1;
        _recoveryKeys = recoveryKeys;
    }

    public static Outcome<RecoveryKeyChain> Create
    (
        UserId userId,
        IReadOnlyCollection<RecoverKeyInput> recoverKeyInputs,
        DateTimeOffset utcNow
    )
    {
        ArgumentNullException.ThrowIfNull(recoverKeyInputs);
        
        if (userId.IsEmpty)
            return RecoveryKeyChainFaults.UserIdRequiredForCreation;

        if (recoverKeyInputs.Count != RecoveryKeyConstants.MaxKeysPerChain)
            return RecoveryKeyChainFaults.InvalidRecoveryKeyCount;

        RecoveryKeyChainId recoveryKeyChainId = RecoveryKeyChainId.New();
        
        List<RecoveryKey> recoveryKeys = new(capacity: RecoveryKeyConstants.MaxKeysPerChain);

        foreach (RecoverKeyInput recoverKeyInput in recoverKeyInputs)
        {
            Outcome<RecoveryKey> recoveryKeyOutcome = RecoveryKey.Create
            (
                recoveryKeyChainId: recoveryKeyChainId,
                identifier: recoverKeyInput.Identifier,
                verifierHash: recoverKeyInput.VerifierHash
            );
            
            if (recoveryKeyOutcome.IsFailure)
                return recoveryKeyOutcome.Fault;
            
            recoveryKeys.Add(recoveryKeyOutcome.Value);
        }

        RecoveryKeyChain recoveryKeyChain = new
        (
            id: recoveryKeyChainId,
            userId: userId,
            utcNow: utcNow,
            recoveryKeys: recoveryKeys
        );

        return recoveryKeyChain;
    }

    public Outcome Rotate
    (
        IReadOnlyCollection<(string identifier, string verifierHash)> newRecoveryKeyPairs,
        DateTimeOffset utcNow
    )
    {
        ArgumentNullException.ThrowIfNull(newRecoveryKeyPairs);
        
        if (newRecoveryKeyPairs.Count != RecoveryKeyConstants.MaxKeysPerChain)
            return RecoveryKeyChainFaults.InvalidRecoveryKeyCount;
        
        List<RecoveryKey> newRecoveryKeys = new(capacity: RecoveryKeyConstants.MaxKeysPerChain);

        foreach ((string identifier, string verifierHash) newRecoveryKeyPair in newRecoveryKeyPairs)
        {
            Outcome<RecoveryKey> newRecoveryKeyOutcome = RecoveryKey.Create
            (
                recoveryKeyChainId: Id,
                identifier: newRecoveryKeyPair.identifier,
                verifierHash: newRecoveryKeyPair.verifierHash
            );
            
            if (newRecoveryKeyOutcome.IsFailure)
                return newRecoveryKeyOutcome.Fault;
            
            newRecoveryKeys.Add(newRecoveryKeyOutcome.Value);
        }
        
        _recoveryKeys.Clear();
        _recoveryKeys.AddRange(newRecoveryKeys);
        LastRotatedAt = utcNow;
        Version += 1;
        
        return Outcome.Success();
    }
}