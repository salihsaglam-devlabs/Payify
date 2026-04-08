using LinkPara.Card.Domain.Entities.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reconciliation;

public class ReconciliationAlertConfiguration : IEntityTypeConfiguration<ReconciliationAlert>
{
    public void Configure(EntityTypeBuilder<ReconciliationAlert> builder)
    {
        ConfigureColumns(builder);

        builder.HasIndex(x => x.FileLineId);
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.EvaluationId);
        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.Severity);
        builder.HasIndex(x => x.AlertStatus);
        builder.HasIndex(x => new { x.AlertStatus, x.CreateDate });

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

    internal static void ConfigureColumns<T>(EntityTypeBuilder<T> builder) where T : ReconciliationAlert
    {
        builder.Property(x => x.FileLineId).HasColumnName("file_line_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.EvaluationId).HasColumnName("evaluation_id");
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(20);
        builder.Property(x => x.AlertType).HasColumnName("alert_type").HasMaxLength(200);
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(2000);
        builder.Property(x => x.AlertStatus).HasColumnName("alert_status").HasConversion<string>().HasMaxLength(32).IsRequired();
    }
}
