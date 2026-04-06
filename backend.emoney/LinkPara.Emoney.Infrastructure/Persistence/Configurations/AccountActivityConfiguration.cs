using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountActivityConfiguration : IEntityTypeConfiguration<AccountActivity>
{
    public void Configure(EntityTypeBuilder<AccountActivity> builder)
    {
        builder.Property(s => s.AccountId).IsRequired();
        builder.Property(s => s.Sender).IsRequired().HasMaxLength(36);
        builder.Property(s => s.Receiver).IsRequired().HasMaxLength(36);
        builder.Property(s => s.TransactionDirection).IsRequired();
        builder.Property(s => s.Amount).IsRequired().HasPrecision(18, 2); 
        builder.Property(s => s.Month).IsRequired();
        builder.Property(s => s.Year).IsRequired();
        builder.Property(s => s.OwnAccount).IsRequired().HasDefaultValue(false);
        builder.Property(s => s.TransferType).HasMaxLength(20);
        builder.HasIndex(s => new { s.AccountId, s.Year, s.Month });
    }
}