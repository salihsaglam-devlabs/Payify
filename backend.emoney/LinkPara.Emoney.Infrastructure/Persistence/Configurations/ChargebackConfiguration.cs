using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ChargebackConfiguration : IEntityTypeConfiguration<Chargeback>
{
    public void Configure(EntityTypeBuilder<Chargeback> builder)
    {
        builder.Property(b => b.UserName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.TransactionId).IsRequired();
        builder.Property(b => b.TransactionType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10);
        builder.Property(b => b.WalletNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Description).IsRequired(false).HasMaxLength(500);
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.MerchantId).IsRequired(false).HasMaxLength(50);
        builder.Property(b => b.MerchantName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.TransactionDate).IsRequired();
        builder.Property(b => b.OrderId).IsRequired().HasMaxLength(50);
    }
}