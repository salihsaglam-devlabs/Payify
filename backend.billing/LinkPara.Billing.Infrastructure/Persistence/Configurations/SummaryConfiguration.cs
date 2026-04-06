using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class SummaryConfiguration : IEntityTypeConfiguration<Summary>
{
    public void Configure(EntityTypeBuilder<Summary> builder)
    {
        builder.Property(s => s.VendorId).IsRequired();
        builder.Property(s => s.ReconciliationDate).IsRequired().HasColumnType("date"); ;
        builder.Property(s => s.ReconciliationStatus).IsRequired();
        builder.Property(b => b.TotalPaymentAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalCancelAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPaymentCount).IsRequired();
        builder.Property(b => b.TotalCancelCount).IsRequired();
        builder.Property(b => b.VendorTotalPaymentAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.VendorTotalCancelAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.VendorTotalPaymentCount).IsRequired();
        builder.Property(b => b.VendorTotalCancelCount).IsRequired();
        builder.Property(b => b.Explanation).HasMaxLength(4000);
    }
}