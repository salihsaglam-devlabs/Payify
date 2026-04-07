using LinkPara.Card.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ProcessExecutionLockConfiguration : IEntityTypeConfiguration<ProcessExecutionLock>
{
    public void Configure(EntityTypeBuilder<ProcessExecutionLock> builder)
    {
        builder.Property(x => x.AcquiredAt).HasColumnName("acquired_at");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.ReleasedAt).HasColumnName("released_at");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.LockName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OwnerId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(50);
        builder.Property(x => x.JobType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Note).HasMaxLength(2000);

        builder.HasIndex(x => new { x.LockName, x.Status, x.ExpiresAt });
        builder.HasIndex(x => x.AcquiredAt);
    }
}
