using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantPreApplicationConfiguration : IEntityTypeConfiguration<MerchantPreApplication>
{
    public void Configure(EntityTypeBuilder<MerchantPreApplication> builder)
    {
        builder.Property(b => b.FullName).IsRequired().HasMaxLength(50);
        builder.Property(b => b.PhoneNumber).IsRequired();
        builder.Property(b => b.Email).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ProductTypes).IsRequired();
        builder.Property(b => b.ApplicationStatus).IsRequired();
        builder.Property(b => b.MonthlyTurnover).IsRequired();
        builder.Property(b => b.Website).IsRequired().HasMaxLength(50);

        builder
            .HasMany(b => b.ApplicationHistories)
            .WithOne(b => b.MerchantPreApplication);
    }
}