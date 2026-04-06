
using LinkPara.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Accounting.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasIndex(s => s.ReferenceId).IsUnique();
        builder.Property(s => s.ReferenceId).HasMaxLength(20);
        builder.Property(s => s.CurrencyCode).HasMaxLength(10);
        builder.Property(s => s.Source).HasMaxLength(20);
        builder.Property(s => s.Destination).HasMaxLength(20);
        builder.Property(s => s.ResultMessage).HasMaxLength(800);
        builder.Property(s => s.Amount).HasPrecision(18, 4);
        builder.Property(s => s.CommissionAmount).HasPrecision(18, 4);
        builder.Property(s => s.BsmvAmount).HasPrecision(18, 4);
        builder.Property(s => s.ReceiverCommissionAmount).HasPrecision(18, 4);
        builder.Property(s => s.ReceiverBsmvAmount).HasPrecision(18, 4);
        builder.Property(s => s.CancelResultMessage).HasMaxLength(800);
        builder.Property(s => s.IbanNumber).HasMaxLength(50);
        builder.Property(s => s.TransactionId).HasMaxLength(40);
        builder.Property(s => s.ReturnAmount).HasPrecision(18, 4);
        builder.Property(s => s.BankCommissionAmount).HasPrecision(18, 4);
        builder.Property(s => s.ChargebackAmount).HasPrecision(18, 4);
        builder.Property(s => s.SuspiciousAmount).HasPrecision(18, 4);
        builder.Property(s => s.DueAmount).HasPrecision(18, 4);
        builder.Property(s => s.ChargebackReturnAmount).HasPrecision(18, 4);
        builder.Property(s => s.SuspiciousReturnAmount).HasPrecision(18, 4);
    }
}
