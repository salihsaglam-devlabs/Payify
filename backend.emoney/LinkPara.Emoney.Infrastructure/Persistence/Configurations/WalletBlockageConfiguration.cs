using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class WalletBlockageConfiguration : IEntityTypeConfiguration<WalletBlockage>
{
    public void Configure(EntityTypeBuilder<WalletBlockage> builder)
    {
        builder.Property(b => b.WalletId).IsRequired(); 
        builder.Property(b => b.WalletNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.AccountName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.WalletCurrencyCode).IsRequired().HasMaxLength(5);
        builder.Property(b => b.CashBlockageAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(b => b.CreditBlockageAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(b => b.OperationType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.BlockageStatus).IsRequired().HasMaxLength(50);
        builder.Property(b => b.BlockageDescription).IsRequired(false).HasMaxLength(1000);        
        builder.Property(b => b.BlockageStartDate).IsRequired();
        builder.Property(b => b.BlockageEndDate).IsRequired(false);
    }
}