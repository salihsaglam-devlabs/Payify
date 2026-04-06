using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CostProfileConfiguration : IEntityTypeConfiguration<CostProfile>
{
    public void Configure(EntityTypeBuilder<CostProfile> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(50);
        builder.Property(b => b.ServiceCommission).IsRequired().HasPrecision(5,3);
        builder.Property(b => b.PointCommission).IsRequired().HasPrecision(5,3);
        builder.Property(b => b.VposId).IsRequired(false);
        builder.Property(b => b.PhysicalPosId).IsRequired(false);
        builder.Property(b => b.ProfileSettlementMode).IsRequired().HasDefaultValue(ProfileSettlementMode.SingleBlock);

        builder
          .HasOne(b => b.Currency)
          .WithMany()
          .HasForeignKey(b => b.CurrencyCode)
          .HasPrincipalKey(b => b.Code)
          .OnDelete(DeleteBehavior.Restrict);

        builder
           .HasMany(b => b.CostProfileItems)
           .WithOne(b => b.CostProfile)
           .IsRequired();
    }
}
