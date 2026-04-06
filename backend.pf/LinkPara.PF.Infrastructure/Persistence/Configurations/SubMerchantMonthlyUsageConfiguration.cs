using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class SubMerchantMonthlyUsageConfiguration : IEntityTypeConfiguration<SubMerchantMonthlyUsage>
{
    public void Configure(EntityTypeBuilder<SubMerchantMonthlyUsage> builder)
    {
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.SubMerchantId).IsRequired();
        builder.HasIndex(b => b.SubMerchantId);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(50);
    }
}