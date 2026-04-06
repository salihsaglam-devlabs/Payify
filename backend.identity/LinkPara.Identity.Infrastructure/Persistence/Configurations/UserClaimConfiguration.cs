using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.Property(u => u.Description).IsRequired(false).HasMaxLength(450);
        builder.Property(u => u.DisplayName).HasMaxLength(450);
        builder.Property(u => u.LastModifiedBy).IsRequired(false).HasMaxLength(100);
        builder.Property(u => u.CreatedBy).IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.UserId);
    }
}