using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.Property(u => u.LastModifiedBy).IsRequired(false).HasMaxLength(100);
        builder.Property(u => u.CreatedBy).IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.RoleId);
    }
}