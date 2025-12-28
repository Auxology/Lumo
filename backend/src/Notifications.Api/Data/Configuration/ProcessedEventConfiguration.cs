using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Api.Entities;

namespace Notifications.Api.Data.Configuration;

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
