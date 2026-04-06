using LinkPara.Fraud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Fraud.Infrastructure.Persistence.Configurations;

public class TriggeredRuleSetKeyConfiguration : IEntityTypeConfiguration<TriggeredRuleSetKey>
{
    public void Configure(EntityTypeBuilder<TriggeredRuleSetKey> builder)
    {
        builder.Property(b => b.Operation).IsRequired().HasMaxLength(50);
        builder.Property(b => b.RuleSetKey).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ComplianceRuleSetKey).HasMaxLength(50);
        builder.Property(b => b.Level).IsRequired().HasMaxLength(50);
    }
}
