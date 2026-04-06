using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class SubMerchantLimitConfiguration : IEntityTypeConfiguration<SubMerchantLimit>
{
    public void Configure(EntityTypeBuilder<SubMerchantLimit> builder)
    {
        builder.Property(b => b.TransactionLimitType).IsRequired();
        builder.Property(b => b.LimitType).IsRequired();
        builder.Property(b => b.Period).IsRequired();
        builder.Property(b => b.MaxAmount).HasPrecision(18, 4);
        builder.Property(b => b.SubMerchantId).IsRequired();
        builder.Property(b => b.Currency).HasMaxLength(50);
    }
}