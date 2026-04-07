using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ReconciliationManualReviewItemConfiguration : IEntityTypeConfiguration<ReconciliationManualReviewItem>
{
    public void Configure(EntityTypeBuilder<ReconciliationManualReviewItem> builder)
    {
        builder.Property(x => x.ReconciliationOperationId).HasColumnName("reconciliation_operation_id").IsRequired();
        builder.Property(x => x.RunId).HasColumnName("run_id").IsRequired();
        builder.Property(x => x.ExecutionGroupId).HasColumnName("execution_group_id").IsRequired();
        builder.Property(x => x.ReviewStatus).HasColumnName("review_status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ReviewNote).HasColumnName("review_note").HasMaxLength(2000);
        builder.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(50);
        builder.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");

        builder.HasIndex(x => x.ReconciliationOperationId).IsUnique();
        builder.HasIndex(x => x.RunId);
        builder.HasIndex(x => x.ExecutionGroupId);
        builder.HasIndex(x => new { x.ReviewStatus, x.CreateDate });

        builder
            .HasOne(x => x.ReconciliationOperation)
            .WithMany()
            .HasForeignKey(x => x.ReconciliationOperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
