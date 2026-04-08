using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationEvaluationConfiguration : IEntityTypeConfiguration<ReconciliationEvaluation>
{
    public void Configure(EntityTypeBuilder<ReconciliationEvaluation> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.GroupId);
        builder.HasOne(x => x.IngestionFileLine)
            .WithMany()
            .HasForeignKey(x => x.FileLineId);
    }

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationEvaluation
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(1000);
        builder.Property(x => x.CreatedOperationCount).HasColumnName("operation_count");
    }
}
