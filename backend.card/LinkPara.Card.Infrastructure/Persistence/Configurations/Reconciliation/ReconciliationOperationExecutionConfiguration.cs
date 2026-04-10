using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationOperationExecutionConfiguration : IEntityTypeConfiguration<ReconciliationOperationExecution>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperationExecution> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => new { x.OperationId, x.AttemptNumber }).IsUnique(false);
        builder.HasIndex(x => x.OperationId);
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

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationOperationExecution
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.FinishedAt).HasColumnName("finished_at");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");
        builder.Property(x => x.ResultCode).HasColumnName("result_code").HasMaxLength(100);
        builder.Property(x => x.ResultMessage).HasColumnName("result_message").HasMaxLength(2000);
        builder.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(4000);
    }
}
