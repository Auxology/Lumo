using System.Diagnostics.CodeAnalysis;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using SharedKernel;

namespace Auth.Domain.Aggregates;

public sealed class RecoveryKeyChain : AggregateRoot<RecoveryKeyChainId>
{
    private readonly List<RecoveryKey> _recoveryKeys = [];
    
    public Guid UserId { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? LastRotatedAt { get; private set; }
    
    public int Version { get; private set; }
    
    public IReadOnlyCollection<RecoveryKey> RecoveryKeys => [.._recoveryKeys];
    
    private RecoveryKeyChain() {} // For EF Core
    
    [SetsRequiredMembers]
    private RecoveryKeyChain
    (
        Guid userId,
        DateTimeOffset utcNow,
        IEnumerable<RecoveryKey> recoveryKeys
    )
    {
        Id = RecoveryKeyChainId.New();
        UserId = userId;
        CreatedAt = utcNow;
        LastRotatedAt = null;
        Version = 1;
        _recoveryKeys = [..recoveryKeys];
    }
}