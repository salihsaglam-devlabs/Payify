using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class TierPermissionConfiguration : IEntityTypeConfiguration<TierPermission>
{
    public void Configure(EntityTypeBuilder<TierPermission> builder)
    {
        builder.Property(s => s.TierLevel).IsRequired();
        builder.Property(s => s.PermissionType).IsRequired();
        builder.Property(s => s.IsEnabled).IsRequired();
    }
}