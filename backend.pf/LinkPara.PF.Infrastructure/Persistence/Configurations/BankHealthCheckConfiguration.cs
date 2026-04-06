using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class BankHealthCheckConfiguration : IEntityTypeConfiguration<BankHealthCheck>
{
    public void Configure(EntityTypeBuilder<BankHealthCheck> builder)
    {
        builder.Property(b => b.AcquireBankId).IsRequired();
        builder.Property(b => b.HealthCheckType).IsRequired();
        builder.Property(b => b.LastCheckDate).IsRequired();
        builder.Property(b => b.AllowedCheckDate).IsRequired();
        builder.Property(b => b.TotalTransactionCount).IsRequired();
        builder.Property(b => b.FailTransactionCount).IsRequired();
        builder.Property(b => b.FailTransactionRate).IsRequired();
    }
}
