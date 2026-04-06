using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantHistoryConfiguration : IEntityTypeConfiguration<MerchantHistory>
{
    public void Configure(EntityTypeBuilder<MerchantHistory> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.PermissionOperationType).IsRequired();
        builder.Property(b => b.NewData).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.OldData).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.Detail).IsRequired().HasMaxLength(256);
        builder.Property(b => b.CreatedNameBy).IsRequired().HasMaxLength(256);
    }
}
