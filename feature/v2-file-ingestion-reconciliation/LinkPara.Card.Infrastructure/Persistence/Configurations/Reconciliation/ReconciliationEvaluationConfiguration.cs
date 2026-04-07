using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationEvaluationConfiguration : IEntityTypeConfiguration<ReconciliationEvaluation>
{
    public void Configure(EntityTypeBuilder<ReconciliationEvaluation> builder)
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Message).HasColumnName("message");
        builder.Property(x => x.CreatedOperationCount).HasColumnName("operation_count");

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.HasIndex(x => x.FileLineId);
        builder.Property(x => x.GroupId).HasColumnName("group_id");

        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.FileLineId);
        builder.HasOne(x => x.IngestionFileLine)
            .WithMany()
            .HasForeignKey(x => x.FileLineId);
    }
}
