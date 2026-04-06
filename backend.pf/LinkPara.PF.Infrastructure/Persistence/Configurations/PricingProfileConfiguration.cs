using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PricingProfileConfiguration : IEntityTypeConfiguration<PricingProfile>
{
    public void Configure(EntityTypeBuilder<PricingProfile> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(50);
        builder.Property(b => b.PricingProfileNumber).HasMaxLength(6);
        builder.Property(b => b.PerTransactionFee).IsRequired().HasPrecision(4,2);
        builder.Property(b => b.IsPaymentToMainMerchant).IsRequired().HasDefaultValue(false);

        builder
         .HasOne(b => b.Currency)
         .WithMany()
         .HasForeignKey(b => b.CurrencyCode)
         .HasPrincipalKey(b => b.Code)
         .OnDelete(DeleteBehavior.Restrict);

        builder
           .HasMany(b => b.PricingProfileItems)
           .WithOne(b => b.PricingProfile)
           .IsRequired();
    }
}
