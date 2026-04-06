using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class SubMerchantConfiguration : IEntityTypeConfiguration<SubMerchant>
{
    public void Configure(EntityTypeBuilder<SubMerchant> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Number).IsRequired().HasMaxLength(15);
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.RejectReason).HasMaxLength(256);
        builder.Property(x => x.ParameterValue).HasMaxLength(100);


        builder
         .HasMany(b => b.SubMerchantDocuments)
         .WithOne(b => b.SubMerchant)
         .IsRequired();

        builder
         .HasMany(b => b.SubMerchantLimits)
         .WithOne(b => b.SubMerchant)
         .IsRequired();

        builder
         .HasMany(b => b.SubMerchantUsers)
         .WithOne(b => b.SubMerchant)
         .IsRequired();
    }
}
