using LinkPara.Card.Domain.Entities.FileIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations;

public class ImportedFileRowConfiguration : IEntityTypeConfiguration<ImportedFileRow>
{
    public void Configure(EntityTypeBuilder<ImportedFileRow> builder)
    {
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.RowType).IsRequired().HasMaxLength(1);
        builder.Property(x => x.RawLine).IsRequired();

        builder.HasIndex(x => new { x.ImportedFileId, x.LineNo }).IsUnique();

        builder
            .HasOne(x => x.ImportedFile)
            .WithMany(x => x.Rows)
            .HasForeignKey(x => x.ImportedFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
