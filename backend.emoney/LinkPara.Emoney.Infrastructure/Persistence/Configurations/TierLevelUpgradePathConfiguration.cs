using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class TierLevelUpgradePathConfiguration : IEntityTypeConfiguration<TierLevelUpgradePath>
{
    public void Configure(EntityTypeBuilder<TierLevelUpgradePath> builder)
    {
        builder.Property(s => s.TierLevel).IsRequired();
        builder.Property(s => s.ValidationType).IsRequired();
        builder.Property(s => s.NextTier).IsRequired();
    }
}