using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationOperationExecutionConfiguration : IEntityTypeConfiguration<ReconciliationOperationExecution>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperationExecution> builder)
    {
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.FinishedAt).HasColumnName("finished_at");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");
        builder.Property(x => x.ResultCode).HasColumnName("result_code");
        builder.Property(x => x.ResultMessage).HasColumnName("result_message");
        builder.Property(x => x.ErrorCode).HasColumnName("error_code");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message");

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestPayload);
        builder.Property(x => x.ResponsePayload);
        builder.Property(x => x.ResultCode).HasMaxLength(100);
        builder.Property(x => x.ResultMessage).HasMaxLength(2000);
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasIndex(x => new { x.OperationId, x.AttemptNumber }).IsUnique(false);
        builder.HasIndex(x => x.OperationId);

        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");

        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => new { x.EvaluationId, x.OperationId });

        builder.HasOne(x => x.IngestionFileLine)
            .WithMany()
            .HasForeignKey(x => x.FileLineId);
        builder.HasOne(x => x.Evaluation)
            .WithMany()
            .HasForeignKey(x => x.EvaluationId);
        builder.HasOne(x => x.Operation)
            .WithMany()
            .HasForeignKey(x => x.OperationId);
    }
}
