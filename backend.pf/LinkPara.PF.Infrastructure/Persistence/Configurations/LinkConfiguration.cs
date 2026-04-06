using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.HasIndex(u => u.LinkCode).IsUnique();
        builder.HasIndex(u => u.ExpiryDate);

        builder.Property(b => b.OrderId).HasMaxLength(24);
        builder.Property(b => b.MerchantName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.MerchantNumber).IsRequired().HasMaxLength(15);
        builder.Property(b => b.SubMerchantName).HasMaxLength(150);
        builder.Property(b => b.SubMerchantNumber).HasMaxLength(15);
        builder.Property(b => b.ExpiryDate).IsRequired();
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.LinkCode).IsRequired().HasMaxLength(24);
        builder.Property(b => b.ProductName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.ProductDescription).IsRequired().HasMaxLength(400);
        builder.Property(b => b.ReturnUrl).HasMaxLength(150);
    }
}
