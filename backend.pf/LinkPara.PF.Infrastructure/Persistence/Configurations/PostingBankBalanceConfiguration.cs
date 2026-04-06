using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations
{
    internal class PostingBankBalanceConfiguration : IEntityTypeConfiguration<PostingBankBalance>
    {
        public void Configure(EntityTypeBuilder<PostingBankBalance> builder)
        {
            builder.Property(b => b.MerchantId).IsRequired();
            builder.Property(b => b.AcquireBankCode).IsRequired();
            builder.Property(b => b.PostingDate).IsRequired().HasColumnType("date");
            builder.Property(b => b.PaymentDate).IsRequired().HasColumnType("date");
            builder.Property(b => b.OldPaymentDate).IsRequired().HasColumnType("date");
            builder.Property(b => b.TransactionDate).IsRequired().HasColumnType("date");
            builder.Property(b => b.Currency).IsRequired();
            builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalPointAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalBankCommissionAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalAmountWithoutBankCommission).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalPfCommissionAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalPfNetCommissionAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalAmountWithoutCommissions).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalParentMerchantCommissionAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalPayingAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TotalReturnAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(b => b.TransactionCount).IsRequired();
            builder.Property(b => b.BatchStatus).IsRequired();
            builder.Property(b => b.ParentMerchantId).IsRequired();
            builder.Property(b => b.BlockageStatus).IsRequired();
            builder.Property(b => b.AccountingStatus).IsRequired();
        }
    }
}
