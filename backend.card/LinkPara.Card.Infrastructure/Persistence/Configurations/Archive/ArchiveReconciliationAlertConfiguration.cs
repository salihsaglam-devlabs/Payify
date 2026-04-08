using LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationAlertConfiguration : IEntityTypeConfiguration<ArchiveReconciliationAlert>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationAlert> builder)
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(20);
        builder.Property(x => x.AlertType).HasColumnName("alert_type").HasMaxLength(200);
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(2000);
        builder.Property(x => x.AlertStatus).HasColumnName("alert_status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
