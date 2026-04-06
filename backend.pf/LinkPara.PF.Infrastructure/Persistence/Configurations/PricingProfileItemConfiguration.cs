using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PricingProfileItemConfiguration : IEntityTypeConfiguration<PricingProfileItem>
{
    public void Configure(EntityTypeBuilder<PricingProfileItem> builder)
    {
        builder.Property(b => b.CommissionRate).IsRequired().HasPrecision(4,2);
        builder.Property(b => b.PricingProfileId).IsRequired();
    }
}
