using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);

        b.Property(u => u.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(UserConstants.MaxDisplayNameLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        b.Property(u => u.EmailAddress)
            .IsRequired()
            .HasMaxLength(UserConstants.MaxEmailAddressLength)
            .HasConversion
            (
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value)
            )
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        b.Property(u => u.AvatarKey)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        b.Property(u => u.IsVerified)
            .IsRequired()
            .HasColumnType("boolean");

        b.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(u => u.UpdatedAt)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(u => u.VerifiedAt)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(u => u.DeletedAt)
            .IsRequired(false)
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.HasIndex(u => u.EmailAddress)
            .IsUnique();

        b.HasIndex(u => u.IsVerified);

        b.HasIndex(u => u.CreatedAt);

        b.HasIndex(u => u.DeletedAt)
            .HasFilter("deleted_at IS NOT NULL");
    }
}