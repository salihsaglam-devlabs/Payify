using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationEvaluationConfiguration : IEntityTypeConfiguration<ArchiveReconciliationEvaluation>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationEvaluation> builder)
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(1000);
        builder.Property(x => x.CreatedOperationCount).HasColumnName("operation_count");
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
