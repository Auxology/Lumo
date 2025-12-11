using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Tests.TestHelpers;

internal sealed class TestAuthDbContext(DbContextOptions<TestAuthDbContext> options)
    : DbContext(options), IAuthDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserToken> UserTokens => Set<UserToken>();

    public DbSet<UserRecoveryCode> UserRecoveryCodes => Set<UserRecoveryCode>();

    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value));

            entity.Property(e => e.EmailAddress).HasConversion(
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value));

            entity.HasMany<UserToken>().WithOne().HasForeignKey(t => t.UserId);
            entity.HasMany<UserRecoveryCode>().WithOne().HasForeignKey(rc => rc.UserId);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<UserRecoveryCode>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasConversion(
                id => id.Value,
                value => SessionId.UnsafeFromGuid(value));

            entity.Property(e => e.UserId).HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value));
        });
    }

    public static TestAuthDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestAuthDbContext(options);
    }
}
