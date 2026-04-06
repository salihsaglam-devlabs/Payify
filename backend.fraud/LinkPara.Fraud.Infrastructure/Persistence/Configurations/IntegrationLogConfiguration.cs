using LinkPara.Fraud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Fraud.Infrastructure.Persistence.Configurations;

public class IntegrationLogConfiguration : IEntityTypeConfiguration<IntegrationLog>
{
    public void Configure(EntityTypeBuilder<IntegrationLog> builder)
    {
        builder.Property(b => b.TransactionMonitoringId).IsRequired();
    }
}
