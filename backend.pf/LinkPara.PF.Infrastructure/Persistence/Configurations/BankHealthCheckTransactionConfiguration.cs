using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class BankHealthCheckTransactionConfiguration : IEntityTypeConfiguration<BankHealthCheckTransaction>
{
    public void Configure(EntityTypeBuilder<BankHealthCheckTransaction> builder)
    {
        builder.Property(b => b.TransactionStatus).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TransactionType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.AcquireBankCode);
        builder.Property(b => b.BankTransactionDate).IsRequired();
    }
}
