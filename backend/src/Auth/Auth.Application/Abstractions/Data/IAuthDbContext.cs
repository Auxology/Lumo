using Auth.Domain.Aggregates;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Abstractions.Data;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Session> Sessions { get; }
    
    DbSet<RecoveryKeyChain> RecoveryKeyChains { get; }
    DbSet<RecoveryKey> RecoveryKeys { get; }
    
    DbSet<LoginRequest> LoginRequests { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}