using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class VposConfiguration : IEntityTypeConfiguration<Vpos>
{
    public void Configure(EntityTypeBuilder<Vpos> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.AcquireBankId).IsRequired();
        builder.Property(b => b.IsOnUsVpos).HasDefaultValue(false);
        builder.Property(b => b.IsInsuranceVpos).HasDefaultValue(false);

        builder
           .HasMany(b => b.CostProfiles)
           .WithOne(b => b.Vpos)
           .IsRequired();

        builder
           .HasMany(b => b.MerchantVposList)
           .WithOne(b => b.Vpos)
           .IsRequired();
    }
}
