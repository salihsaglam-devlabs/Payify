using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class WalletPaymentRequestConfiguration : IEntityTypeConfiguration<WalletPaymentRequest>
{
    public void Configure(EntityTypeBuilder<WalletPaymentRequest> builder)
    {
        builder.Property(t => t.PaymentReferenceId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(t => t.SenderWalletNo).IsRequired().HasMaxLength(20);
        builder.Property(t => t.ReceiverWalletNo).IsRequired().HasMaxLength(20);
        builder.Property(t => t.SenderName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.ReceiverName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.TransactionDate).IsRequired();
        builder.Property(s => s.IsLoggedIn).IsRequired().HasDefaultValue(false);

        builder.HasIndex(t => t.PaymentReferenceId);
    }
    
}