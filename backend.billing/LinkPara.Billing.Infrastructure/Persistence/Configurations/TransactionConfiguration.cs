using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(b => b.InstitutionId).IsRequired();
        builder.Property(b => b.BillAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.CommissionAmount).HasPrecision(18, 4);
        builder.Property(b => b.Currency).HasMaxLength(50).IsRequired();
        builder.Property(b => b.BillId).HasMaxLength(50).IsRequired();
        builder.Property(b => b.BillNumber).HasMaxLength(50).IsRequired();
        builder.Property(b => b.SubscriptionNumber1).HasMaxLength(100).IsRequired();
        builder.Property(b => b.SubscriptionNumber2).HasMaxLength(100);
        builder.Property(b => b.SubscriptionNumber3).HasMaxLength(100);
        builder.Property(b => b.VoucherNumber).HasMaxLength(100);
        builder.Property(b => b.ReferenceId).HasMaxLength(100);
        builder.Property(b => b.BillDate).HasColumnType("date");
        builder.Property(b => b.BillDueDate).HasColumnType("date").IsRequired();
        builder.Property(b => b.ServiceRequestId).HasMaxLength(100).IsRequired();
        builder.Property(b => b.VendorId).IsRequired();
        builder.Property(b => b.TransactionStatus).IsRequired();
        builder.Property(b => b.PayeeFullName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.SubscriberName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.PayeeEmail).HasMaxLength(100);
        builder.Property(b => b.PayeeMobile).HasMaxLength(100).IsRequired();
        builder.Property(b => b.ErrorCode).HasMaxLength(100);
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.PaymentDate).HasColumnType("date");
        builder.Property(b => b.WalletNumber).HasMaxLength(50);
        builder.HasIndex(b => b.BillNumber);
        builder.HasIndex(b => b.SubscriptionNumber1);
        builder.HasIndex(b => b.PayeeFullName);
        builder.HasIndex(b => b.TransactionStatus);
        builder.HasIndex(b => b.InstitutionId);
        builder.HasIndex(b => b.VendorId);
    }
}