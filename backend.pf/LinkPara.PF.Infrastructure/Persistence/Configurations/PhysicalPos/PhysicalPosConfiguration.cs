using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations.PhysicalPos;

public class PhysicalPosConfiguration : IEntityTypeConfiguration<Domain.Entities.PhysicalPos.PhysicalPos>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.PhysicalPos.PhysicalPos> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(200);
        builder.Property(b => b.AcquireBankId).IsRequired();
        builder.Property(b => b.PfMainMerchantId).HasMaxLength(200);

        builder
           .HasMany(b => b.CostProfiles)
           .WithOne(b => b.PhysicalPos)
           .IsRequired();

        builder
           .HasMany(b => b.MerchantPhysicalPosList)
           .WithOne(b => b.PhysicalPos)
           .IsRequired();
    }
}
