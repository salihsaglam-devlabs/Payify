using LinkPara.Fraud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Fraud.Infrastructure.Persistence.Configurations;

public class TransactionMonitoringConfiguration : IEntityTypeConfiguration<TransactionMonitoring>
{
    public void Configure(EntityTypeBuilder<TransactionMonitoring> builder)
    {
        builder.Property(b => b.CommandName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Module).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TransferRequest).IsRequired();
        builder.Property(b => b.CurrencyCode).IsRequired().HasMaxLength(10);
        builder.Property(b => b.ReceiverNumber).HasMaxLength(50);
        builder.Property(b => b.ReceiverName).HasMaxLength(100);
        builder.Property(b => b.SenderNumber).HasMaxLength(50);
        builder.Property(b => b.SenderName).HasMaxLength(100);
        builder.Property(b => b.CommandJson).IsRequired();
        builder.Property(b => b.ErrorMessage).HasMaxLength(300);
        builder.Property(b => b.ErrorCode).HasMaxLength(100);
        builder.Property(b => b.TransactionId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ClientIpAddress).HasMaxLength(50);
    }
}
