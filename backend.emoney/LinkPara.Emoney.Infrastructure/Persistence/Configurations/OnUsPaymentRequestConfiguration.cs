using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class OnUsPaymentRequestConfiguration : IEntityTypeConfiguration<OnUsPaymentRequest>
{
    public void Configure(EntityTypeBuilder<OnUsPaymentRequest> builder)
    {
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.UserName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.MerchantName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.MerchantNumber).IsRequired().HasMaxLength(15);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10);
        builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
        builder.Property(b => b.ChargebackDescription).HasMaxLength(500);
        builder.Property(b => b.PhoneCode).IsRequired().HasMaxLength(5);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(15);        
        builder.Property(b => b.WalletNumber).HasMaxLength(20);
        builder.Property(b => b.WalletId);
        builder.Property(b => b.TransactionId);
        builder.Property(b => b.TransactionDate);
        builder.Property(b => b.ErrorCode).HasMaxLength(30);
        builder.Property(b => b.ErrorMessage).HasMaxLength(256);
        builder.Property(b => b.CancelDescription).HasMaxLength(300);
        builder.Property(b => b.ConversationId).HasMaxLength(100);
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.ExpireDate).IsRequired();
        builder.Property(b => b.RequestDate).IsRequired();
        builder.Property(b => b.MerchantTransactionId).HasMaxLength(50);
    }
}
