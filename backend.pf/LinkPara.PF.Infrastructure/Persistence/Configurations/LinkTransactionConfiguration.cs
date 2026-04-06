

using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class LinkTransactionConfiguration : IEntityTypeConfiguration<LinkTransaction>
{
    public void Configure(EntityTypeBuilder<LinkTransaction> builder)
    {
        builder.HasIndex(u => u.LinkCode);
        builder.HasIndex(u => u.TransactionDate);

        builder.Property(b => b.OrderId).HasMaxLength(24);
        builder.Property(b => b.TransactionType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ThreeDSessionId).HasMaxLength(200);
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.InstallmentCount).IsRequired();
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.LinkCode).IsRequired().HasMaxLength(24);
    }
}

