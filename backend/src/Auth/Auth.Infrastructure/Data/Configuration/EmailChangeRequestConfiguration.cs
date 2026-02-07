using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class EmailChangeRequestConfiguration : IEntityTypeConfiguration<EmailChangeRequest>
{
    public void Configure(EntityTypeBuilder<EmailChangeRequest> b)
    {
        b.HasKey(ecr => ecr.Id);

        b.Property(ecr => ecr.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => EmailChangeRequestId.UnsafeFrom(s)
            )
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({EmailChangeRequestId.Length})");

        b.Property(ecr => ecr.UserId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.ComplexProperty(ecr => ecr.CurrentEmailAddress, ea =>
        {
            ea.Property(e => e.Value)
                .HasColumnName("current_email_address")
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);
        });

        b.ComplexProperty(ecr => ecr.NewEmailAddress, ea =>
        {
            ea.Property(e => e.Value)
                .HasColumnName("new_email_address")
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);
        });

        b.Property(ecr => ecr.OtpTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        b.ComplexProperty(ecr => ecr.Fingerprint, fp => fp.ConfigureFingerprint());

        b.Property(ecr => ecr.CreatedAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(ecr => ecr.ExpiresAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(ecr => ecr.CompletedAt)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(ecr => ecr.CancelledAt)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.HasIndex(ecr => ecr.UserId);
        b.HasIndex(ecr => ecr.CreatedAt);
        b.HasIndex(ecr => ecr.ExpiresAt);

        b.HasIndex(ecr => new { ecr.UserId, ecr.ExpiresAt })
            .HasFilter("completed_at IS NULL AND cancelled_at IS NULL");
    }
}