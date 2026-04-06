using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantTransactionConfiguration : IEntityTypeConfiguration<MerchantTransaction>
{
    public void Configure(EntityTypeBuilder<MerchantTransaction> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.VposId).IsRequired();
        builder.Property(b => b.ConversationId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.IpAddress).HasMaxLength(50);
        builder.Property(b => b.TransactionStatus).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TransactionType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TransactionStartDate).IsRequired();
        builder.Property(b => b.TransactionEndDate);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PointAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PointCommissionRate).HasPrecision(18, 4);
        builder.Property(b => b.PointCommissionAmount).HasPrecision(18, 4);
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.InstallmentCount).IsRequired();
        builder.Property(b => b.BinNumber).IsRequired().HasMaxLength(10);
        builder.Property(b => b.CardNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.SubMerchantName).HasMaxLength(150);
        builder.Property(b => b.SubMerchantNumber).HasMaxLength(15);
        builder.Property(b => b.HasCvv).IsRequired();
        builder.Property(b => b.HasExpiryDate).IsRequired();
        builder.Property(b => b.IsInternational).IsRequired();
        builder.Property(b => b.IsAmex).IsRequired();
        builder.Property(b => b.IsReverse).IsRequired();
        builder.Property(b => b.IsOnUsPayment).IsRequired();
        builder.Property(b => b.IsInsurancePayment).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.ReverseDate);
        builder.Property(b => b.IsReturn).IsRequired();
        builder.Property(b => b.ReturnDate);
        builder.Property(b => b.ReturnAmount).HasPrecision(18,4);
        builder.Property(b => b.ReturnedTransactionId).HasMaxLength(50);
        builder.Property(b => b.IsPreClose).IsRequired();
        builder.Property(b => b.PreCloseDate);
        builder.Property(b => b.PreCloseTransactionId).HasMaxLength(50);
        builder.Property(b => b.Is3ds).IsRequired();
        builder.Property(b => b.ThreeDSessionId).HasMaxLength(200);
        builder.Property(b => b.BankCommissionRate).IsRequired().HasPrecision(5,3);
        builder.Property(b => b.BankCommissionAmount).IsRequired().HasPrecision(18,4);
        builder.Property(b => b.IssuerBankCode).IsRequired();
        builder.Property(b => b.AcquireBankCode).IsRequired();
        builder.Property(b => b.CardTransactionType).HasColumnType("varchar(50)");
        builder.Property(b => b.IntegrationMode).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ResponseCode).HasMaxLength(20);
        builder.Property(b => b.ResponseDescription).HasMaxLength(1000);
        builder.Property(b => b.LanguageCode).HasMaxLength(100);
        builder.Property(b => b.CardType).IsRequired();
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.SuspeciousDescription).HasMaxLength(500);
        builder.Property(b => b.LastChargebackActivityDate).IsRequired();
        builder.Property(b => b.MerchantCustomerName).HasMaxLength(200);
        builder.Property(b => b.Description).HasMaxLength(300);
        builder.Property(b => b.MerchantCustomerPhoneNumber).HasMaxLength(30);
        builder.Property(b => b.MerchantCustomerPhoneCode).HasMaxLength(10);
        builder.Property(b => b.CardHolderName).HasMaxLength(200);
        builder.Property(b => b.CreatedNameBy).HasMaxLength(200);
        builder.Property(b => b.PfCommissionAmount).HasPrecision(18, 4);
        builder.Property(b => b.PfNetCommissionAmount).HasPrecision(18, 4);
        builder.Property(b => b.PfCommissionRate).HasPrecision(18, 4);
        builder.Property(b => b.PfPerTransactionFee).HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutCommissions).HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutBankCommission).HasPrecision(18, 4);
        builder.Property(b => b.ParentMerchantCommissionAmount).HasPrecision(18, 4);
        builder.Property(b => b.ParentMerchantCommissionRate).HasPrecision(18, 4);
        builder.Property(b => b.AmountWithoutParentMerchantCommission).HasPrecision(18, 4);
        builder.Property(b => b.BsmvAmount).HasPrecision(18, 4);
        builder.Property(b => b.ProvisionNumber).HasMaxLength(50);
        builder.Property(b => b.VposName).HasMaxLength(100);
        builder.Property(b => b.ServiceCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.ServiceCommissionRate).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.BlockageStatus).IsRequired();
        builder.Property(b => b.PfTransactionSource).IsRequired().HasDefaultValue(PfTransactionSource.VirtualPos);
        builder.Property(b => b.EndOfDayStatus).IsRequired().HasDefaultValue(EndOfDayStatus.Pending);
        builder.Property(b => b.CardHolderIdentityNumber).HasMaxLength(15);

        builder.Property(b => b.TransactionDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(b => b.BankPaymentDate)
            .IsRequired()
            .HasColumnType("date");

        builder.HasIndex(x => new { x.BatchStatus, x.RecordStatus });
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.PostingItemId);

        builder
       .HasOne(s => s.AcquireBank)
       .WithMany()
       .HasForeignKey(s => s.AcquireBankCode)
       .HasPrincipalKey(s => s.Code)
       .OnDelete(DeleteBehavior.Restrict);

        builder
         .HasOne(s => s.IssuerBank)
         .WithMany()
         .HasForeignKey(s => s.IssuerBankCode)
         .HasPrincipalKey(s => s.Code)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
