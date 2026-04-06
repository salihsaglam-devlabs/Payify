using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.Property(b => b.PaymentType).IsRequired();
        builder.Property(b => b.VendorId).IsRequired();
        builder.Property(b => b.Rate).IsRequired().HasPrecision(18,4);
        builder.Property(b => b.Fee).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.MinValue).IsRequired();
        builder.Property(b => b.MaxValue).IsRequired();
    }
}