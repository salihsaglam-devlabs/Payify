using LinkPara.Scheduler.API.Commons.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Scheduler.API.Persistence.Configurations;

public class CronJobConfiguration : IEntityTypeConfiguration<CronJob>
{
    public void Configure(EntityTypeBuilder<CronJob> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.CronExpression).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(300);
        builder.Property(t => t.Module).HasMaxLength(100).IsRequired();
        builder.Property(t => t.CronJobType).HasMaxLength(50).IsRequired();
        builder.Property(t => t.HttpType).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Uri).HasMaxLength(500);
    }
}