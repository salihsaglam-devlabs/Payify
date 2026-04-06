using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CardTokenConfiguration : IEntityTypeConfiguration<CardToken>
{
    public void Configure(EntityTypeBuilder<CardToken> builder)
    {
        builder.Property(b => b.Token).IsRequired().HasMaxLength(50);
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.ExpiryDate).IsRequired();
        builder.HasIndex(b => b.ExpiryDate);
        builder.Property(b => b.CvvEncrypted).HasMaxLength(300);
        builder.Property(b => b.CardNumberEncrypted).IsRequired().HasMaxLength(300);
        builder.Property(b => b.ExpireDateEncrypted).IsRequired().HasMaxLength(300);
    }
}
