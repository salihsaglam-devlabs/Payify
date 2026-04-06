using LinkPara.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.PhoneNumber);
        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.UserStatus);
        builder.HasIndex(u => u.UserType);
        builder.HasIndex(u => u.IdentityNumber).IsUnique();

        builder.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.IdentityNumber).HasMaxLength(50);
        builder.Property(u => u.LastModifiedBy).HasMaxLength(100);
        builder.Property(u => u.CreatedBy).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PhoneNumber).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PhoneCode).HasMaxLength(10).IsRequired();

        builder.Property(u => u.ExternalCustomerId).HasMaxLength(400);
        builder.Property(u => u.ExternalPersonId).HasMaxLength(400);
        builder.Property(u => u.AmlReferenceNumber).HasMaxLength(150);
        builder.HasOne(u => u.LoginLastActivity).WithOne(a => a.User)
            .HasForeignKey<User>(u => u.LoginLastActivityId)
            .IsRequired(false);

        builder.Ignore(u => u.DomainEvents);

        builder
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<IdentityUserRole<Guid>>(
                userRole => userRole.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired(),
                userRole => userRole.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired());

    }
}