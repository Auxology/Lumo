using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class RecoveryRequestConfiguration : IEntityTypeConfiguration<RecoveryRequest>
{
    public void Configure(EntityTypeBuilder<RecoveryRequest> b)
    {
        b.HasKey(rr => rr.Id);

        b.Property(rr => rr.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => RecoveryRequestId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({RecoveryRequestId.Length})");

        b.Property(rr => rr.UserId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(rr => rr.TokenKey)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.ComplexProperty(rr => rr.NewEmailAddress, ea =>
        {
            ea.Property(e => e.Value)
                .HasColumnName("new_email_address")
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnType("varchar");
        });

        b.Property(rr => rr.OtpTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.Property(rr => rr.MagicLinkTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.ComplexProperty(rr => rr.Fingerprint, fp => fp.ConfigureFingerprint());

        b.Property(rr => rr.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(rr => rr.ExpiresAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(rr => rr.NewEmailVerifiedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.Property(rr => rr.CompletedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.HasIndex(rr => rr.TokenKey).IsUnique();
        b.HasIndex(rr => rr.UserId);
        b.HasIndex(rr => rr.CreatedAt);
        b.HasIndex(rr => rr.ExpiresAt);
    }
}