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
        IReadOnlyCollection<(string identifier, string verifierHash)> recoveryKeyPairs,
        DateTimeOffset utcNow
    )
    {
        ArgumentNullException.ThrowIfNull(recoveryKeyPairs);
        
        if (userId.IsEmpty)
            return RecoveryKeyChainFaults.UserIdRequiredForCreation;

        if (recoveryKeyPairs.Count != RecoveryKeyConstants.MaxKeysPerChain)
            return RecoveryKeyChainFaults.InvalidRecoveryKeyCount;

        RecoveryKeyChainId recoveryKeyChainId = RecoveryKeyChainId.New();
        
        List<RecoveryKey> recoveryKeys = new(capacity: RecoveryKeyConstants.MaxKeysPerChain);

        foreach ((string identifier, string verifierHash) recoveryKeysPair in recoveryKeyPairs)
        {
            Outcome<RecoveryKey> recoveryKeyOutcome = RecoveryKey.Create
            (
                recoveryKeyChainId: recoveryKeyChainId,
                identifier: recoveryKeysPair.identifier,
                verifierHash: recoveryKeysPair.verifierHash
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
}