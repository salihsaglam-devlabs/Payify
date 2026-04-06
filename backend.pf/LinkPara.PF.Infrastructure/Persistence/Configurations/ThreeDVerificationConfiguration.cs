using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class ThreeDVerificationConfiguration : IEntityTypeConfiguration<ThreeDVerification>
{
    public void Configure(EntityTypeBuilder<ThreeDVerification> builder)
    {
        builder.Property(b => b.TransactionType).IsRequired();
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.CurrentStep).IsRequired().HasMaxLength(50);
        builder.Property(b => b.MdStatus).HasMaxLength(2);
        builder.Property(b => b.Md).HasMaxLength(256);
        builder.Property(b => b.MdErrorMessage).HasMaxLength(256);
        builder.Property(b => b.Xid).HasMaxLength(50);
        builder.Property(b => b.Eci).HasMaxLength(50);
        builder.Property(b => b.Cavv).HasMaxLength(50);
        builder.Property(b => b.PayerTxnId).HasMaxLength(100);
        builder.Property(b => b.TxnStat).HasMaxLength(50);
        builder.Property(b => b.ThreeDStatus).HasMaxLength(50);
        builder.Property(b => b.CardToken).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PointAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.CallbackUrl).HasMaxLength(250);
        builder.Property(b => b.HashKey).HasMaxLength(200);
        builder.Property(b => b.BankResponseCode).HasMaxLength(40);
        builder.Property(b => b.BankResponseDescription).HasMaxLength(256);
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.MerchantCode).IsRequired().HasMaxLength(50);
        builder.Property(b => b.SubMerchantCode).IsRequired().HasMaxLength(50);
        builder.Property(b => b.SubMerchantTerminalNo).IsRequired().HasMaxLength(20);
        builder.Property(b => b.AcquireBankCode).IsRequired().HasMaxLength(3);
        builder.Property(b => b.IssuerBankCode).IsRequired().HasMaxLength(3);
        builder.Property(b => b.BinNumber).HasMaxLength(8);
        builder.Property(b => b.VposId).IsRequired();
        builder.Property(b => b.CostProfileItemId).IsRequired();
        builder.Property(b => b.BankCommissionRate).IsRequired().HasPrecision(4, 2);
        builder.Property(b => b.BankCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.ConversationId).HasMaxLength(50);
        builder.Property(b => b.BankPacket).HasMaxLength(500);
    }
}
