using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountKycChangeConfiguration : IEntityTypeConfiguration<AccountKycChange>
{
    public void Configure(EntityTypeBuilder<AccountKycChange> builder)
    {
        builder.Property(s => s.AccountId).IsRequired();
        builder.Property(s => s.ValidationType).IsRequired();
        builder.Property(s => s.OldKycLevel).IsRequired();
        builder.Property(s => s.NewKycLevel).IsRequired();
        builder.Property(s => s.IsUpgraded).IsRequired();
    }
}