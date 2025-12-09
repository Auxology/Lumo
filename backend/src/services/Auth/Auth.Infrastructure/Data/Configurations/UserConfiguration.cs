using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(UserConstants.MaxDisplayNameLength)
            .IsRequired();

        builder.Property(x => x.EmailAddress)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value))
            .HasMaxLength(UserConstants.MaxEmailLength)
            .IsRequired();
        
        builder.Property(x => x.AvatarKey)
            .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
            .IsRequired(false);
        
        builder.Property(x => x.EmailVerified)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.HasIndex(x => x.EmailAddress)
            .IsUnique();

        builder.HasIndex(x => x.CreatedAt);
        
        builder.Ignore(x => x.DomainEvents);
        
        
        builder.OwnsMany(u => u.UserTokens, tokenBuilder =>
        {
              tokenBuilder.WithOwner()
                  .HasForeignKey("UserId");

              tokenBuilder.HasKey(x => x.Id);

              tokenBuilder.Property(x => x.Id)
                  .ValueGeneratedOnAdd()
                  .IsRequired();

              tokenBuilder.Property(x => x.UserId)
                  .HasConversion(
                      id => id.Value,
                      value => UserId.UnsafeFromGuid(value))
                  .HasColumnType("uuid")
                  .IsRequired();

              tokenBuilder.Property(x => x.OtpTokenHash)
                  .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
                  .IsRequired();

              tokenBuilder.Property(x => x.MagicLinkTokenHash)
                  .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
                  .IsRequired();

              tokenBuilder.Property(x => x.IpAddress)
                  .HasMaxLength(AuthDbConstants.MaxIpAddressLength)
                  .IsRequired();

              tokenBuilder.Property(x => x.UserAgent)
                  .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
                  .IsRequired();

              tokenBuilder.Property(x => x.CreatedAt)
                  .HasColumnType("timestamptz")
                  .IsRequired();

              tokenBuilder.Property(x => x.ExpiresAt)
                  .HasColumnType("timestamptz")
                  .IsRequired();

              tokenBuilder.Property(t => t.UsedAt)
                  .HasColumnType("timestamptz")
                  .IsRequired(false);

              tokenBuilder.HasIndex(x => new { x.UserId, x.CreatedAt });
              
              tokenBuilder.HasIndex(x => x.ExpiresAt);
        });

        builder.OwnsMany(u => u.UserRecoveryCodes, codeBuilder =>
        {
              codeBuilder.WithOwner()
                  .HasForeignKey("UserId");

              codeBuilder.HasKey(x => x.Id);

              codeBuilder.Property(x => x.Id)
                  .ValueGeneratedOnAdd()
                  .IsRequired();

              codeBuilder.Property(x => x.UserId)
                  .HasConversion(
                      id => id.Value,
                      value => UserId.UnsafeFromGuid(value))
                  .HasColumnType("uuid")
                  .IsRequired();

              codeBuilder.Property(x => x.RecoveryCodeHash)
                  .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
                  .IsRequired();

              codeBuilder.Property(x => x.IsRevoked)
                  .IsRequired();

              codeBuilder.Property(x => x.CreatedAt)
                  .HasColumnType("timestamptz")
                  .IsRequired();

              codeBuilder.Property(x => x.UsedAt)
                  .HasColumnType("timestamptz")
                  .IsRequired(false);

              codeBuilder.HasIndex(x => new { x.UserId, x.IsRevoked });
              
              codeBuilder.HasIndex(x => x.CreatedAt);
        });
    }
}