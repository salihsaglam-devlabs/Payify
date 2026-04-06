using LinkPara.PF.Domain.Entities.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class DeviceInventoryConfiguration : IEntityTypeConfiguration<DeviceInventory>
{
    public void Configure(EntityTypeBuilder<DeviceInventory> builder)
    {
        builder.Property(b => b.SerialNo).HasMaxLength(200);

        builder.HasIndex(x => new
        {
            x.DeviceModel,
            x.PhysicalPosVendor,
            x.DeviceType,
            x.SerialNo
        })
            .IsUnique();

        builder
           .HasMany(b => b.MerchantPhysicalDevices)
           .WithOne(b => b.DeviceInventory)
           .IsRequired();
    }
}
