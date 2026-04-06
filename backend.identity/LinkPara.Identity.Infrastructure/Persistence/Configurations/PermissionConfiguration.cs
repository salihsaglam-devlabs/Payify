using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(u => u.ClaimValue).IsUnique();
        builder.Property(u => u.ClaimValue).IsRequired(true).HasMaxLength(250);
        builder.Property(u => u.ClaimType).HasMaxLength(200);
        builder.Property(u => u.Module).HasMaxLength(200);
        builder.Property(u => u.DisplayName).HasMaxLength(200);
        builder.Property(u => u.Description).IsRequired(false).HasMaxLength(450);
        builder.Property(u => u.NormalizedClaimValue).IsRequired(true).HasMaxLength(250);
    }
}