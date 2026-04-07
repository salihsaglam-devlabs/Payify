using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.CurrentBalance).HasPrecision(18, 2);
        builder.Property(t => t.PreBalance).HasPrecision(18, 2);
        builder.Property(t => t.Tag).HasMaxLength(200);
        builder.Property(t => t.TagTitle).HasMaxLength(50);
        builder.Property(t => t.Description).HasMaxLength(300);
        builder.Property(t => t.PaymentType).HasMaxLength(100);
        builder.Property(t => t.ExternalReferenceId).HasMaxLength(50);
        builder.Property(t => t.TransactionDate).IsRequired();
        builder.Property(t => t.CurrencyCode).IsRequired();
        builder.Property(t => t.Channel).HasMaxLength(300);
        builder.Property(t => t.SenderAccountNumber).HasMaxLength(26);
        builder.Property(t => t.IpAddress).HasMaxLength(50);

        builder.Property(t => t.ReceiverName).HasMaxLength(100);
        builder.Property(t => t.SenderName).HasMaxLength(100);
        builder.Property(t => t.ReceiptNumber).HasMaxLength(30);
        builder.Property(t => t.CustomerTransactionId).HasMaxLength(50);
        builder.Property(t => t.PaymentChannel).HasMaxLength(300);
        builder.Property(t => t.UsedCreditAmount).HasPrecision(18, 2);
        builder.HasIndex(t => t.WithdrawRequestId);
        builder.HasIndex(t => t.RelatedTransactionId);
        builder.HasIndex(t => t.TransactionStatus);
        builder.HasIndex(t => t.CurrencyCode);
        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.TransactionType);

        builder
            .HasOne(s => s.Currency)
            .WithMany()
            .HasForeignKey(s => s.CurrencyCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}