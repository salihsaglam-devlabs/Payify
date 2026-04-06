using LinkPara.Fraud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Fraud.Infrastructure.Persistence.Configurations;

public class TriggeredRuleConfiguration : IEntityTypeConfiguration<TriggeredRule>
{
    public void Configure(EntityTypeBuilder<TriggeredRule> builder)
    {
        builder.Property(b => b.RuleKey).IsRequired().HasMaxLength(300);
        builder.Property(b => b.TransactionMonitoringId).IsRequired();
    }
}
