using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class TransactionReferenceCounterConfiguration : IEntityTypeConfiguration<TransactionReferenceCounter>
{
    public void Configure(EntityTypeBuilder<TransactionReferenceCounter> builder)
    {
        builder.HasIndex(s => s.TransactionReferenceInt).IsUnique();
        builder.Property(s => s.TransactionReferenceInt).ValueGeneratedOnAdd();

        builder.Property(s => s.TransactionReferenceInt).IsRequired();
        builder.Property(s => s.TransactionReferenceGuid).IsRequired();
    }
}