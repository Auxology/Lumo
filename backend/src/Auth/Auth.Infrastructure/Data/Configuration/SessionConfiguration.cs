using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s => s.Id)
            .HasConversion
            (
                id => id.Value,
                guid => SessionId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");
        
        b.Property(s => s.UserId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");
        
        b.ComplexProperty(s => s.Fingerprint, fp =>
        {
            fp.Property(f => f.IpAddress)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxIpAddressLength)
                .HasColumnType("varchar");

            fp.Property(f => f.UserAgent)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxUserAgentLength)
                .HasColumnType("varchar");

            fp.Property(f => f.Timezone)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxTimezoneLength)
                .HasColumnType("varchar");

            fp.Property(f => f.Language)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxLanguageLength)
                .HasColumnType("varchar");

            fp.Property(f => f.ComputedHash)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
                .HasColumnType("varchar");

            fp.Property(f => f.NormalizedBrowser)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxNormalizedBrowserLength)
                .HasColumnType("varchar");

            fp.Property(f => f.NormalizedOs)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxNormalizedOsLength)
                .HasColumnType("varchar");
        });
        
        b.Property(s => s.RefreshTokenKey)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");
        
        b.Property(s => s.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");
        
        b.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(s => s.ExpiresAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(s => s.LastRefreshedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");
        
        b.Property(s => s.RevokeReason)
            .IsRequired(false)
            .HasConversion<string>()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength);

        b.Property(s => s.RevokedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.HasIndex(s => s.UserId);
        
        b.HasIndex(s => s.RefreshTokenKey)
            .IsUnique();
        
        b.HasIndex(s => s.ExpiresAt);
        
        b.HasIndex(s => s.RevokedAt);
        
        b.HasIndex(s => new { s.UserId, s.RevokedAt });
        
        b.HasIndex(s => new { s.ExpiresAt, s.RevokedAt });
    }
}