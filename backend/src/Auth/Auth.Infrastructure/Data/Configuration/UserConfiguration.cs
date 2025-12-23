using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            .HasColumnType("varchar");

        b.Property(u => u.EmailAddress)
            .IsRequired()
            .HasConversion
            (
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value)
            )
            .HasColumnType("varchar");

        b.Property(u => u.AvatarKey)
            .IsRequired(false)
            .HasColumnType("varchar");
        
        b.Property(u => u.IsVerified)
            .IsRequired()
            .HasColumnType("boolean");

        b.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(u => u.UpdatedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");
        
        b.Property(u => u.VerifiedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.HasIndex(u => u.EmailAddress)
            .IsUnique();
        
        b.HasIndex(u => u.IsVerified);
        
        b.HasIndex(u => u.CreatedAt);
    }
}