using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates;
using Auth.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Data;

internal sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options), IAuthDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<RecoveryKeyChain> RecoveryKeyChains { get; set; }
    public DbSet<RecoveryKey> RecoveryKeys { get; set; }
    public DbSet<LoginRequest> LoginRequests { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}