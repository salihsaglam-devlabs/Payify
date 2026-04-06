using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(u => u.Name).IsUnique();
        builder.Property(u => u.Name).IsRequired(true).HasMaxLength(250);
        builder.Property(u => u.NormalizedName).HasMaxLength(100);
        builder.Property(u => u.ConcurrencyStamp).HasMaxLength(450);
        builder.Property(u => u.LastModifiedBy).IsRequired(false).HasMaxLength(100);
        builder.Property(u => u.CreatedBy).IsRequired().HasMaxLength(50);
    }
}