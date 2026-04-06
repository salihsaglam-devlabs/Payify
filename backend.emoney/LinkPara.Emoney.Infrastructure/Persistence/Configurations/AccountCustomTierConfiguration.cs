using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountCustomTierConfiguration : IEntityTypeConfiguration<AccountCustomTier>
{
    public void Configure(EntityTypeBuilder<AccountCustomTier> builder)
    {
        builder.Property(s => s.AccountId).IsRequired();
        builder.Property(s => s.TierLevelId).IsRequired();
        builder.Property(s => s.AccountName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PhoneNumber).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PhoneCode).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Email).IsRequired().HasMaxLength(250);
        builder.HasIndex(s => s.TierLevelId);
    }
}