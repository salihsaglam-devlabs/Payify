using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class OnUsPaymentConfiguration : IEntityTypeConfiguration<OnUsPayment>
{
    public void Configure(EntityTypeBuilder<OnUsPayment> builder)
    {
        builder.Property(b => b.OrderId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Amount).HasPrecision(18, 4).IsRequired();
        builder.Property(b => b.CallbackUrl).IsRequired().HasMaxLength(250);
        builder.Property(b => b.Name).HasMaxLength(150);
        builder.Property(b => b.Surname).HasMaxLength(150);
        builder.Property(b => b.Email).HasMaxLength(256);
        builder.Property(b => b.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(b => b.PhoneCode).HasMaxLength(10).IsRequired();
        builder.Property(b => b.MerchantName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.MerchantNumber).IsRequired().HasMaxLength(15);
        builder.Property(b => b.ExpiryDate).IsRequired();
        builder.Property(b => b.WalletNumber).HasMaxLength(10);
        builder.Property(b => b.EmoneyReferenceNumber).HasMaxLength(256);
        
        builder.HasIndex(u => u.MerchantTransactionId);
        builder.HasIndex(u => u.ExpiryDate);
        builder.HasIndex(u => u.WebhookStatus);
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.PaymentStatus);
    }
}