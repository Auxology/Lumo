using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class LoginRequestConfiguration : IEntityTypeConfiguration<LoginRequest>
{
    public void Configure(EntityTypeBuilder<LoginRequest> b)
    {
        b.HasKey(lr => lr.Id);

        b.Property(lr => lr.Id)
            .HasConversion
            (
                id => id.Value,
                s => LoginRequestId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({LoginRequestId.Length})");

        b.Property(lr => lr.UserId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(lr => lr.TokenKey)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.Property(lr => lr.OtpTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.Property(lr => lr.MagicLinkTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.ComplexProperty(lr => lr.Fingerprint, fp => fp.ConfigureFingerprint());

        b.Property(lr => lr.CreatedAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(lr => lr.ExpiresAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(lr => lr.ConsumedAt)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.HasIndex(lr => lr.UserId);

        b.HasIndex(lr => lr.TokenKey)
            .IsUnique();

        b.HasIndex(lr => lr.ExpiresAt);

        b.HasIndex(lr => lr.ConsumedAt);

        b.HasIndex(lr => new { lr.TokenKey, lr.ConsumedAt, lr.ExpiresAt });
    }
}