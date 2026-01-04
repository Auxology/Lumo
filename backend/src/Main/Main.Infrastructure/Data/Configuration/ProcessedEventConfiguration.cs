using Main.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> b)
    {
        b.HasKey(e => e.EventId);

        b.Property(e => e.EventId)
            .IsRequired()
            .HasColumnType("uuid");

        b.Property(e => e.ProcessedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
    }
}
