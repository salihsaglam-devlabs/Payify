using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingBatchStatusConfiguration : IEntityTypeConfiguration<PostingBatchStatus>
{
    public void Configure(EntityTypeBuilder<PostingBatchStatus> builder)
    {
        builder.Property(p => p.PostingBatchLevel).IsRequired();
        builder.Property(p => p.BatchSummary).IsRequired().HasMaxLength(200);
        builder.Property(p => p.IsCriticalError).IsRequired();
        builder.Property(p => p.StartTime).IsRequired();
        builder.Property(p => p.FinishTime);
        builder.Property(b => b.PostingDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.BatchOrder).IsRequired();

        builder.HasIndex(p => p.PostingDate);
    }
}