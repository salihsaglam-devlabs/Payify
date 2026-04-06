using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ReturnTransactionRequestConfiguration : IEntityTypeConfiguration<ReturnTransactionRequest>
{
    public void Configure(EntityTypeBuilder<ReturnTransactionRequest> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.ReceiverName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.ReceiverIbanNumber).IsRequired().HasMaxLength(26);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.CreateDate).IsRequired();
        builder.Property(t => t.ReceiverBankCode).IsRequired();
        builder.Property(t => t.TransactionBankCode).IsRequired().HasDefaultValue(0);
        builder.Property(t => t.TransferType).IsRequired();
        builder.Property(t => t.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(t => t.IncomingTransactionId).IsRequired();
        builder.Property(t => t.IsProcessed).IsRequired();
    }
}