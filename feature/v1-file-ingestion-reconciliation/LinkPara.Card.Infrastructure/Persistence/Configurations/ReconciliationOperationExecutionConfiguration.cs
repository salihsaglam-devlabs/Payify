using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ReconciliationOperationExecutionConfiguration : IEntityTypeConfiguration<ReconciliationOperationExecution>
{
    public void Configure(EntityTypeBuilder<ReconciliationOperationExecution> builder)
    {
        builder.Property(x => x.ReconciliationOperationId).HasColumnName("reconciliation_operation_id").IsRequired();
        builder.Property(x => x.ExecutionGroupId).HasColumnName("execution_group_id").IsRequired();
        builder.Property(x => x.AttemptNo).HasColumnName("attempt_no").IsRequired();
        builder.Property(x => x.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(x => x.EndedAt).HasColumnName("ended_at");
        builder.Property(x => x.Outcome).HasColumnName("outcome").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
        builder.Property(x => x.ErrorText).HasColumnName("error_text").HasMaxLength(2000);
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");

        builder.HasIndex(x => x.ReconciliationOperationId);
        builder.HasIndex(x => x.ExecutionGroupId);
        builder.HasIndex(x => new { x.ReconciliationOperationId, x.AttemptNo }).IsUnique();
        builder.HasIndex(x => new { x.Outcome, x.CreateDate });

        builder
            .HasOne(x => x.ReconciliationOperation)
            .WithMany()
            .HasForeignKey(x => x.ReconciliationOperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
