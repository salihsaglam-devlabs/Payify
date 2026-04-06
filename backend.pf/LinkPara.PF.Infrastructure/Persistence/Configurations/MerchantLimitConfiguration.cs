using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantLimitConfiguration : IEntityTypeConfiguration<MerchantLimit>
{
    public void Configure(EntityTypeBuilder<MerchantLimit> builder)
    {
        builder.Property(b => b.TransactionLimitType).IsRequired();
        builder.Property(b => b.LimitType).IsRequired();
        builder.Property(b => b.Period).IsRequired();
        builder.Property(b => b.MaxAmount).HasPrecision(18, 4);
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.Currency).HasMaxLength(50);
    }
}
