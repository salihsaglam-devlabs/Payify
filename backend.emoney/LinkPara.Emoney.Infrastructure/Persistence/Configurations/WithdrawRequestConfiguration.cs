using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class WithdrawRequestConfiguration : IEntityTypeConfiguration<WithdrawRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawRequest> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.ReceiverName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.ReceiverIbanNumber).IsRequired().HasMaxLength(26);
        builder.Property(t => t.IsReceiverIbanOwned).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(150);
        builder.Property(t => t.WalletNumber).IsRequired().HasMaxLength(10);
        builder.Property(t => t.CreateDate).IsRequired();
        builder.Property(t => t.ReceiverBankCode).IsRequired();
        builder.Property(t => t.TransactionBankCode).IsRequired().HasDefaultValue(0);
        builder.Property(t => t.TransferType).IsRequired();
        builder.Property(t => t.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(t => t.InternalTransactionId).IsRequired();
        builder.HasIndex(t => t.WalletNumber);
        builder.HasIndex(t => new { t.WithdrawStatus, t.RecordStatus });
        builder.HasIndex(t => t.CreateDate);
        builder.Property(t => t.ReceiverBankName).HasMaxLength(200);
        builder.Property(t => t.TransactionBankName).HasMaxLength(200);
        builder.Property(t => t.IsProcessed).IsRequired();
    }
}