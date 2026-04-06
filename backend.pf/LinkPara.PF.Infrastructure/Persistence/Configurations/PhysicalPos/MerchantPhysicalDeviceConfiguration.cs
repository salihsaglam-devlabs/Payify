using LinkPara.PF.Domain.Entities.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class MerchantPhysicalDeviceConfiguration : IEntityTypeConfiguration<MerchantPhysicalDevice>
{
    public void Configure(EntityTypeBuilder<MerchantPhysicalDevice> builder)
    {
        builder.Property(b => b.FiscalNo).HasMaxLength(200);
        builder.Property(b => b.DeviceInventoryId).IsRequired();
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.OwnerPspNo).HasMaxLength(100);
        builder.Property(b => b.OwnerTerminalId).HasMaxLength(50);

        builder
          .HasMany(b => b.MerchantPhysicalPosList)
          .WithOne(b => b.MerchantPhysicalDevice)
          .IsRequired();

        builder
         .HasMany(b => b.DeviceApiKeys)
         .WithOne(b => b.MerchantPhysicalDevice)
         .IsRequired();
    }
}
