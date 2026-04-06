using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class MerchantPhysicalPosConfiguration : IEntityTypeConfiguration<MerchantPhysicalPos>
{
    public void Configure(EntityTypeBuilder<MerchantPhysicalPos> builder)
    {
        builder.Property(b => b.MerchantPhysicalDeviceId).IsRequired();
        builder.Property(b => b.PhysicalPosId).IsRequired();
        builder.Property(b => b.PosMerchantId).HasMaxLength(50);
        builder.Property(b => b.PosTerminalId).HasMaxLength(50);
        builder.Property(b => b.BkmReferenceNumber).HasMaxLength(50);
        builder.Property(b => b.DeviceTerminalStatus).IsRequired().HasDefaultValue(DeviceTerminalStatus.Unknown);
    }
}
