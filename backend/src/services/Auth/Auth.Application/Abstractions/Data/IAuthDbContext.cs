using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Abstractions.Data;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<UserToken> UserTokens { get; }
    
    DbSet<UserRecoveryCode> UserRecoveryCodes { get; }
    
    DbSet<Session> Sessions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}