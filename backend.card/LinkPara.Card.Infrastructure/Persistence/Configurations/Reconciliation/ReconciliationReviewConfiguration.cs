using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationReviewConfiguration : IEntityTypeConfiguration<ReconciliationReview>
{
    public void Configure(EntityTypeBuilder<ReconciliationReview> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => new { x.Decision, x.CreateDate });

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

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationReview
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.ReviewerId).HasColumnName("reviewer_id");
        builder.Property(x => x.Decision).HasColumnName("decision").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(2000);
        builder.Property(x => x.DecisionAt).HasColumnName("decision_at");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.ExpirationAction).HasColumnName("expiration_action").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ExpirationFlowAction).HasColumnName("expiration_flow_action").HasConversion<string>().HasMaxLength(32).IsRequired();
    }
}
