using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations
{
    public class MerchantDailyUsageConfiguration : IEntityTypeConfiguration<MerchantDailyUsage>
    {
        public void Configure(EntityTypeBuilder<MerchantDailyUsage> builder)
        {
            builder.Property(b => b.Amount).HasPrecision(18, 4);
            builder.Property(b => b.MerchantId).IsRequired();
            builder.HasIndex(b => b.MerchantId);
            builder.Property(b => b.Currency).IsRequired().HasMaxLength(50);
        }
    }
}
