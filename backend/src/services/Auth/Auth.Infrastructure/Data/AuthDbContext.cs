using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Data;

internal sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options), IAuthDbContext
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserToken> UserTokens { get; set; }
    
    public DbSet<UserRecoveryCode> UserRecoveryCodes { get; set; }
    
    public DbSet<Session> Sessions { get; set; }
}