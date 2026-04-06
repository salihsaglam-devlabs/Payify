using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingBalanceConfiguration : IEntityTypeConfiguration<PostingBalance>
{
    public void Configure(EntityTypeBuilder<PostingBalance> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.TransactionDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.PostingDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.PaymentDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.OldPaymentDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.MoneyTransferPaymentDate).HasColumnType("date");
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPointAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalBankCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalAmountWithoutBankCommission).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPfCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPfNetCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalAmountWithoutCommissions).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalDueAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalDueTransferAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalChargebackAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalChargebackCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalChargebackTransferAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalSuspiciousAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalSuspiciousCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalSuspiciousTransferAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalExcessReturnAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalExcessReturnTransferAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalExcessReturnOnCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalNegativeBalanceAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPayingAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalParentMerchantCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.MoneyTransferStatus).IsRequired();
        builder.Property(b => b.BatchStatus).IsRequired().HasMaxLength(50);
        builder.Property(b => b.BlockageStatus).IsRequired().HasMaxLength(50);
        builder.Property(b => b.RetryCount).IsRequired();
        builder.Property(b => b.TransactionCount).IsRequired();
        builder.Property(b => b.PostingBalanceType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Iban).HasMaxLength(26);
        builder.Property(b => b.WalletNumber).HasMaxLength(26);
        builder.Property(b => b.MoneyTransferBankName).HasMaxLength(50);
        builder.Property(b => b.TransactionDate).HasColumnType("date");
        builder.Property(b => b.BTransStatus).IsRequired();
        builder.Property(b => b.AccountingStatus).IsRequired();
        builder.Property(b => b.PostingPaymentChannel).IsRequired();
        builder.Property(b => b.ParentMerchantId).IsRequired();

        builder.HasIndex(b => new { b.MoneyTransferStatus, b.BTransStatus });
    }
}