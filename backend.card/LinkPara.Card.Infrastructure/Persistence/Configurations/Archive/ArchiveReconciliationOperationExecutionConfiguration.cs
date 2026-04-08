using LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationOperationExecutionConfiguration : IEntityTypeConfiguration<ArchiveReconciliationOperationExecution>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationOperationExecution> builder)
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.FinishedAt).HasColumnName("finished_at");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");
        builder.Property(x => x.ResultCode).HasColumnName("result_code").HasMaxLength(100);
        builder.Property(x => x.ResultMessage).HasColumnName("result_message").HasMaxLength(2000);
        builder.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(4000);
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
