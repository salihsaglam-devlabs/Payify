using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class PricingCommercialConfiguration  : IEntityTypeConfiguration<PricingCommercial>
{
    public void Configure(EntityTypeBuilder<PricingCommercial> builder)
    {
        builder.Property(s => s.MaxDistinctSenderAmount).HasDefaultValue(0).HasPrecision(18, 2);
        builder.Property(s => s.MaxDistinctSenderCountWithAmount).HasDefaultValue(0);
        builder.Property(s => s.MaxDistinctSenderCount).HasDefaultValue(0);
        builder.Property(s => s.PricingCommercialType).IsRequired().HasMaxLength(10);
        builder.Property(s => s.PricingCommercialStatus).IsRequired().HasMaxLength(20);
        builder.Property(s => s.CommissionRate).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.ActivationDate).IsRequired();
        builder.Property(s => s.CurrencyCode).IsRequired().HasMaxLength(3);
    }
}