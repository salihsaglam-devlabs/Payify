using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class PhysicalPosEndOfDayConfiguration : IEntityTypeConfiguration<PhysicalPosEndOfDay>
{
    public void Configure(EntityTypeBuilder<PhysicalPosEndOfDay> builder)
    {
        builder.Property(b => b.MerchantName).HasMaxLength(150);
        builder.Property(b => b.MerchantNumber).HasMaxLength(15);
        builder.Property(b => b.BatchId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.PosMerchantId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.PosTerminalId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Date).IsRequired();
        builder.Property(b => b.SaleAmount).HasPrecision(18, 4);
        builder.Property(b => b.VoidAmount).HasPrecision(18, 4);
        builder.Property(b => b.RefundAmount).HasPrecision(18, 4);
        builder.Property(b => b.InstallmentSaleAmount).HasPrecision(18, 4);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10);
        builder.Property(b => b.Vendor).HasMaxLength(20);
        builder.Property(b => b.SerialNumber).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Status).IsRequired().HasDefaultValue(EndOfDayStatus.Pending);
    }
}
