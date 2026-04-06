using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingAdditionalTransactionConfiguration : IEntityTypeConfiguration<PostingAdditionalTransaction>
{
    public void Configure(EntityTypeBuilder<PostingAdditionalTransaction> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.TransactionType).IsRequired();
        builder.Property(b => b.CardNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.InstallmentCount).IsRequired();
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PointAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BankCommissionRate).IsRequired().HasPrecision(4, 2);
        builder.Property(b => b.BankCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutBankCommission).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PfCommissionRate).IsRequired().HasPrecision(4, 2);
        builder.Property(b => b.PfCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PfPerTransactionFee).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PfNetCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.ParentMerchantCommissionAmount).HasPrecision(18, 4);
        builder.Property(b => b.ParentMerchantCommissionRate).HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutParentMerchantCommission).HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutCommissions).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BatchStatus).IsRequired();
        builder.Property(b => b.MerchantTransactionId).IsRequired();
        builder.Property(b => b.PricingProfileNumber).IsRequired();
        builder.Property(b => b.VposId).IsRequired();
        builder.Property(b => b.TransactionDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.PostingDate).IsRequired().HasColumnType("date");   
        builder.Property(b => b.PaymentDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.OldPaymentDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.BlockageStatus).IsRequired();
        builder.Property(b => b.AcquireBankCode).IsRequired();
        builder.Property(b => b.MerchantDeductionId).IsRequired();
        builder.Property(b => b.RelatedPostingBalanceId).IsRequired();
        builder.Property(b => b.SubMerchantId).IsRequired();
        builder.Property(b => b.SubMerchantName).HasMaxLength(150);
        builder.Property(b => b.SubMerchantNumber).HasMaxLength(15);
        builder.Property(b => b.EasySubMerchantName).HasMaxLength(150);
        builder.Property(b => b.EasySubMerchantNumber).HasMaxLength(15);

        builder.HasIndex(b => new { b.BatchStatus, b.RecordStatus });
        builder.Property(b => b.TransactionStartDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.TransactionEndDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.ConversationId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.PfTransactionSource).IsRequired().HasDefaultValue(PfTransactionSource.VirtualPos);
        builder.Property(b => b.MerchantPhysicalPosId).IsRequired();
    }
}