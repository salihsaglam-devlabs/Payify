using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class SynchroniztionLogConfiguration : IEntityTypeConfiguration<SynchronizationLog>
{
    public void Configure(EntityTypeBuilder<SynchronizationLog> builder)
    {
        builder.Property(s => s.ItemId).IsRequired();
        builder.Property(s => s.ItemName).IsRequired();
        builder.Property(s => s.VendorId).IsRequired();
        builder.Property(s => s.SynchronizationDate).IsRequired();
        builder.Property(s => s.SynchronizationType).IsRequired();
        builder.Property(s => s.SynchronizationType).IsRequired();
        builder.Property(s => s.SynchronizationItem).IsRequired();
    }
}
