using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class TimeoutTransactionConfiguration : IEntityTypeConfiguration<TimeoutTransaction>
{
    public void Configure(EntityTypeBuilder<TimeoutTransaction> builder)
    {
        builder.Property(b => b.VendorId).IsRequired();
        builder.Property(b => b.InstitutionId).IsRequired();
        builder.Property(b => b.BillAmount).IsRequired();
        builder.Property(b => b.CommissionAmount).IsRequired();
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(20);
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
        builder.Property(b => b.WalletNumber).HasMaxLength(50);
        builder.Property(b => b.PaymentDate).HasColumnType("date");
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.TransactionId).IsRequired();
        builder.Property(b => b.SubscriberName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.PayeeFullName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.PayeeEmail).HasMaxLength(100);
        builder.Property(b => b.PayeeMobile).HasMaxLength(100).IsRequired();
        builder.Property(b => b.ServiceRequestId).HasMaxLength(100).IsRequired();
        builder.Property(b => b.ErrorCode).HasMaxLength(100);
        builder.Property(b => b.TimeoutTransactionType).IsRequired();

        builder.HasIndex(b => new { b.TimeoutTransactionStatus, b.NextTryTime, b.RetryCount });
    }
}