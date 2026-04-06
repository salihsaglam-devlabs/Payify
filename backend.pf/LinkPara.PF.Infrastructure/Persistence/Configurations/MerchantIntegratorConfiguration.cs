using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantIntegratorConfiguration : IEntityTypeConfiguration<MerchantIntegrator>
{
    public void Configure(EntityTypeBuilder<MerchantIntegrator> builder)
    {
        builder.Property(b => b.CommissionRate).IsRequired().HasPrecision(4, 2);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);

        builder
      .HasMany(b => b.Merchants)
      .WithOne(b => b.MerchantIntegrator)
      .OnDelete(DeleteBehavior.Restrict);
    }
}