using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveReconciliationReviewConfiguration : IEntityTypeConfiguration<ArchiveReconciliationReview>
{
    public void Configure(EntityTypeBuilder<ArchiveReconciliationReview> builder)
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.ReviewerId).HasColumnName("reviewer_id");
        builder.Property(x => x.Decision).HasColumnName("decision").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(2000);
        builder.Property(x => x.DecisionAt).HasColumnName("decision_at");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.ExpirationAction).HasColumnName("expiration_action").HasMaxLength(32).IsRequired();
        builder.Property(x => x.ExpirationFlowAction).HasColumnName("expiration_flow_action").HasMaxLength(32).IsRequired();
        builder.Property(x => x.ArchiveRunId).HasColumnName("archive_run_id");
        builder.Property(x => x.ArchivedAt).HasColumnName("archived_at");
        builder.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.ArchiveRunId);
    }
}
