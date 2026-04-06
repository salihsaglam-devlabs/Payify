using LinkPara.PF.Domain.Entities.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class DeviceInventoryHistoryConfiguration : IEntityTypeConfiguration<DeviceInventoryHistory>
{
    public void Configure(EntityTypeBuilder<DeviceInventoryHistory> builder)
    {
        builder.Property(b => b.NewData).HasMaxLength(500);
        builder.Property(b => b.OldData).HasMaxLength(500);
        builder.Property(b => b.Detail).HasMaxLength(256);
        builder.Property(b => b.CreatedNameBy).HasMaxLength(256);
        builder.Property(b => b.DeviceInventoryId).IsRequired();
    }
}
