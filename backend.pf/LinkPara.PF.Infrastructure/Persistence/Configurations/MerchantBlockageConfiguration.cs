using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantBlockageConfiguration : IEntityTypeConfiguration<MerchantBlockage>
{
    public void Configure(EntityTypeBuilder<MerchantBlockage> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BlockageAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.RemainingAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.MerchantBlockageStatus).IsRequired();
    }
}