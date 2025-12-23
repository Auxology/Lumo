using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;
using SharedKernel;

namespace Auth.Domain.Entities;

public sealed class RecoveryKey : Entity<int>
{
    public RecoveryKeyChainId RecoveryKeyChainId { get; private set; }

    public string Identifier { get; private set; } = string.Empty;
    
    public string VerifierHash { get; private set; } = string.Empty;
    
    public bool IsUsed { get; private set; }
    
    public DateTimeOffset? UsedAt { get; private set; }
    
    public Fingerprint? Fingerprint { get; private set; }
    
    private RecoveryKey() {} // For EF Core

    [SetsRequiredMembers]
    private RecoveryKey
    (
        RecoveryKeyChainId recoveryKeyChainId,
        string identifier,
        string verifierHash
    )
    {
        RecoveryKeyChainId = recoveryKeyChainId;
        Identifier = identifier;
        VerifierHash = verifierHash;
        IsUsed = false;
        UsedAt = null;
        Fingerprint = null;
    }

    public static Outcome<RecoveryKey> Create
    (
        RecoveryKeyChainId recoveryKeyChainId,
        string identifier,
        string verifierHash
    )
    {
        if (recoveryKeyChainId.IsEmpty)
            return RecoveryKeyFaults.RecoveryKeyChainIdRequiredForCreation;

        if (string.IsNullOrWhiteSpace(identifier))
            return RecoveryKeyFaults.IdentifierRequiredForCreation;
        
        if (string.IsNullOrWhiteSpace(verifierHash))
            return RecoveryKeyFaults.VerifierHashRequiredForCreation;

        RecoveryKey recoveryKey = new
        (
            recoveryKeyChainId: recoveryKeyChainId,
            identifier: identifier,
            verifierHash: verifierHash
        );

        return recoveryKey;
    }
}