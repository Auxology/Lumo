using Main.Domain.Aggregates;
using Main.Domain.Constants;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RabbitMQ.Client;

using SharedKernel.Infrastructure.Data;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class SharedChatConfiguration : IEntityTypeConfiguration<SharedChat>
{
    public void Configure(EntityTypeBuilder<SharedChat> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => SharedChatId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({SharedChatId.Length})");

        b.Property(s => s.SourceChatId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                s => ChatId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({ChatId.Length})");

        b.Property(s => s.OwnerId)
            .IsRequired()
            .HasColumnType("uuid");

        b.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(ChatConstants.MaxTitleLength)
            .HasColumnType($"varchar({ChatConstants.MaxTitleLength})");

        b.Property(s => s.ModelId)
            .IsRequired()
            .HasMaxLength(ChatConstants.MaxModelIdLength)
            .HasColumnType($"varchar({ChatConstants.MaxModelIdLength})");

        b.Property(s => s.ViewCount)
            .IsRequired()
            .HasColumnType("integer");

        b.Property(s => s.SnapshotAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.OwnsMany(s => s.SharedChatMessages, mb =>
        {
            // Override the table name for the owned entity
            mb.ToTable("shared_chat_messages");

            mb.HasKey("SharedChatId", nameof(SharedChatMessage.SequenceNumber));

            mb.Property(m => m.SequenceNumber)
                .ValueGeneratedNever()
                .IsRequired()
                .HasColumnType("integer");

            mb.Property(m => m.MessageRole)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength);

            mb.Property(m => m.MessageContent)
                .IsRequired()
                .HasColumnType("text");

            mb.Property(m => m.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamptz");
        });

        b.HasIndex(s => s.OwnerId);

        b.HasIndex(s => s.SourceChatId);
    }
}