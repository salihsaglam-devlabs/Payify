using LinkPara.Epin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Epin.Infrastructure.Persistence.Configurations;

public class ReconciliationSummaryConfiguration : IEntityTypeConfiguration<ReconciliationSummary>
{
    public void Configure(EntityTypeBuilder<ReconciliationSummary> builder)
    {
        builder.Property(s => s.Message).HasMaxLength(300);
        builder.Property(t => t.ExternalTotal).HasPrecision(18, 4);
        builder.Property(t => t.OrderTotal).HasPrecision(18, 4);

        builder
            .HasMany(s => s.ReconciliationDetails)
            .WithOne(s => s.ReconciliationSummary)
            .IsRequired();
    }
}
