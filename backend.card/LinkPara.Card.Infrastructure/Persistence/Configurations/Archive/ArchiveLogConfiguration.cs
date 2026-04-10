using LinkPara.Card.Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Archive;

public class ArchiveLogConfiguration : IEntityTypeConfiguration<ArchiveLog>
{
    public void Configure(EntityTypeBuilder<ArchiveLog> builder)
    {
        builder.Property(x => x.IngestionFileId).HasColumnName("ingestion_file_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(2000);
        builder.Property(x => x.FailureReasonsJson).HasColumnName("failure_reasons_json");
        builder.Property(x => x.FilterJson).HasColumnName("filter_json");
        builder.HasIndex(x => x.IngestionFileId);
    }
}

