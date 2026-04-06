using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class BulkTransferDetailConfiguration : IEntityTypeConfiguration<BulkTransferDetail>
{
    public void Configure(EntityTypeBuilder<BulkTransferDetail> builder)
    {
        builder.Property(s => s.FullName).HasMaxLength(250).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(s => s.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(400);
        builder.Property(s => s.ExceptionMessage).HasMaxLength(400);
        builder.Property(s => s.Receiver).HasMaxLength(50).IsRequired();

        builder
            .HasOne(s => s.Transaction)
            .WithOne(s => s.BulkTransferDetail)
            .HasForeignKey<BulkTransferDetail>(s => s.TransactionId);
    }
}