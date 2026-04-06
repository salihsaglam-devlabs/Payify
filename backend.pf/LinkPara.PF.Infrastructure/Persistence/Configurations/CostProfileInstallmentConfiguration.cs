using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CostProfileInstallmentConfiguration : IEntityTypeConfiguration<CostProfileInstallment>
{
    public void Configure(EntityTypeBuilder<CostProfileInstallment> builder)
    {
        builder.Property(b => b.CostProfileItemId).IsRequired();
        builder.Property(b => b.BlockedDayNumber).IsRequired();
        builder.Property(b => b.InstallmentSequence).IsRequired();
        
        builder
            .HasOne(s => s.CostProfileItem)
            .WithMany(i => i.CostProfileInstallments)
            .HasForeignKey(s => s.CostProfileItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(s => new { s.CostProfileItemId, s.InstallmentSequence })
            .IsUnique();
    }
}
