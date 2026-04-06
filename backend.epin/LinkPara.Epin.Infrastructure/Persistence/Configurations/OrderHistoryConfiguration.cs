using LinkPara.Epin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Epin.Infrastructure.Persistence.Configurations;

public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
{
    public void Configure(EntityTypeBuilder<OrderHistory> builder)
    {
        builder.Property(t => t.Total).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.Pin).HasMaxLength(100).IsRequired();
        builder.Property(t => t.UnitPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(t => t.Discount).HasPrecision(18, 4).IsRequired();
        builder.Property(t => t.Vat).HasPrecision(18, 4).IsRequired();
    }
}
