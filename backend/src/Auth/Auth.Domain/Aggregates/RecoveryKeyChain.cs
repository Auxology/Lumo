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
    
    public string KeyIdentifier { get; private set; } = string.Empty;
    
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
        string keyIdentifier,
        DateTimeOffset utcNow,
        List<RecoveryKey> recoveryKeys
    )
    {
        Id = id;
        UserId = userId;
        KeyIdentifier = keyIdentifier;
        CreatedAt = utcNow;
        LastRotatedAt = null;
        Version = 1;
        _recoveryKeys = recoveryKeys;
    }

    public static Outcome<RecoveryKeyChain> Create
    (
        UserId userId,
        string keyIdentifier,
        IReadOnlyList<string> verifierHashes,
        DateTimeOffset utcNow
    )
    {
        ArgumentNullException.ThrowIfNull(verifierHashes);
        
        if (userId.IsEmpty)
            return RecoveryKeyChainFaults.UserIdRequiredForCreation;

        if (string.IsNullOrWhiteSpace(keyIdentifier))
            return RecoveryKeyChainFaults.KeyIdentifierRequiredForCreation;

        if (verifierHashes.Count != RecoveryKeyConstants.MaxKeysPerChain)
            return RecoveryKeyChainFaults.InvalidRecoveryKeyCount;

        RecoveryKeyChainId recoveryKeyChainId = RecoveryKeyChainId.New();
        
        List<RecoveryKey> recoveryKeys = new(capacity: RecoveryKeyConstants.MaxKeysPerChain);

        for (int i = 0; i < verifierHashes.Count; i++)
        {
            Outcome<RecoveryKey> recoveryKeyOutcome = RecoveryKey.Create
            (
                recoveryKeyChainId: recoveryKeyChainId,
                verifierHash: verifierHashes[i]
            );

            if (!recoveryKeyOutcome.IsSuccess)
                return recoveryKeyOutcome.Fault;

            recoveryKeys.Add(recoveryKeyOutcome.Value);
        }

        RecoveryKeyChain recoveryKeyChain = new
        (
            id: recoveryKeyChainId,
            userId: userId,
            keyIdentifier: keyIdentifier,
            utcNow: utcNow,
            recoveryKeys: recoveryKeys
        );

        return recoveryKeyChain;
    }
}