using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class SectorMappingConfiguration : IEntityTypeConfiguration<SectorMapping>
{
    public void Configure(EntityTypeBuilder<SectorMapping> builder)
    {
        builder.Property(b => b.SectorId).IsRequired();
        builder.Property(b => b.VendorSectorId).IsRequired().HasMaxLength(100);
        builder.Property(b => b.VendorId).IsRequired();

        builder.HasIndex(b => new { b.SectorId, b.VendorId });
    }
}