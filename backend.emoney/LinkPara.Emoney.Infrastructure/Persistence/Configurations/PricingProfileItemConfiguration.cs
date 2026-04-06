using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class PricingProfileItemConfiguration : IEntityTypeConfiguration<PricingProfileItem>
{
    public void Configure(EntityTypeBuilder<PricingProfileItem> builder)
    {
        builder.Property(s => s.Fee).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MinAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.MaxAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.CommissionRate).IsRequired().HasPrecision(18, 2);
        builder.HasIndex(s => s.PricingProfileId);
    }
}
