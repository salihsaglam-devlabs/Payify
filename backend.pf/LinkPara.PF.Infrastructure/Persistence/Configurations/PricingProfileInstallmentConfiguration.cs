using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PricingProfileInstallmentConfiguration : IEntityTypeConfiguration<PricingProfileInstallment>
{
    public void Configure(EntityTypeBuilder<PricingProfileInstallment> builder)
    {
        builder.Property(b => b.PricingProfileItemId).IsRequired();
        builder.Property(b => b.BlockedDayNumber).IsRequired();
        builder.Property(b => b.InstallmentSequence).IsRequired();
        
        builder
            .HasOne(s => s.PricingProfileItem)
            .WithMany(i => i.PricingProfileInstallments)
            .HasForeignKey(s => s.PricingProfileItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(s => new { s.PricingProfileItemId, s.InstallmentSequence })
            .IsUnique();
    }
}
