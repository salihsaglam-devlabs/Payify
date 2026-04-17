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
        builder.Property(x => x.FileLineId);
        builder.Property(x => x.GroupId);
        builder.Property(x => x.EvaluationId);
        builder.Property(x => x.OperationId);
        builder.Property(x => x.ReviewerId);
        builder.Property(x => x.Decision).HasConversion<string>().IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.DecisionAt);
        builder.Property(x => x.ExpiresAt);
        builder.Property(x => x.ExpirationAction).HasConversion<string>().IsRequired();
        builder.Property(x => x.ExpirationFlowAction).HasConversion<string>().IsRequired();
    }
}
