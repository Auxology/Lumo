using Main.Application.Abstractions.Data;
using Main.Domain.Aggregates;
using Main.Domain.Entities;
using Main.Domain.ReadModels;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Main.Infrastructure.Data;

internal sealed class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options), IMainDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
