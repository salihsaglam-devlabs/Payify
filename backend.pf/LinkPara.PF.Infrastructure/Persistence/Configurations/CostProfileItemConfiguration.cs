using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CostProfileItemConfiguration : IEntityTypeConfiguration<CostProfileItem>
{
    public void Configure(EntityTypeBuilder<CostProfileItem> builder)
    {
        builder.Property(b => b.CommissionRate).IsRequired().HasPrecision(5,3);
        builder.Property(b => b.CostProfileId).IsRequired();
    }
}