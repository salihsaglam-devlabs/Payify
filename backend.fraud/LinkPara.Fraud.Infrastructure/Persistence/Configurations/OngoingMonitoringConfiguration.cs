using LinkPara.Fraud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Fraud.Infrastructure.Persistence.Configurations;

public class OngoingMonitoringConfiguration : IEntityTypeConfiguration<OngoingMonitoring>
{
    public void Configure(EntityTypeBuilder<OngoingMonitoring> builder)
    {
        builder.Property(b => b.SearchName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.ScanId).HasMaxLength(50);
    }
}
