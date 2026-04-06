using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class PricingProfileConfiguration : IEntityTypeConfiguration<PricingProfile>
{
    public void Configure(EntityTypeBuilder<PricingProfile> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ActivationDateStart).IsRequired();
        builder.Property(s => s.CurrencyCode).IsRequired().HasMaxLength(10);
        builder.Property(s => s.TransferType).IsRequired();
        builder.HasIndex(s => s.TransferType);
        builder.Property(s => s.Status).IsRequired();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.BankCode);
        builder.HasIndex(t => t.CurrencyCode);

        builder
         .HasOne(s => s.Currency)
         .WithMany()
         .HasForeignKey(s => s.CurrencyCode)
         .HasPrincipalKey(s => s.Code)
         .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(s => s.Bank)
            .WithMany()
            .HasForeignKey(s => s.BankCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(s=>s.PricingProfileItems)
            .WithOne(s=>s.PricingProfile)
            .IsRequired();
    }
}
