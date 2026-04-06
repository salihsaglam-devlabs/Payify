using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class AgreementDocumentVersionConfiguration : IEntityTypeConfiguration<AgreementDocumentVersion>
{
    public void Configure(EntityTypeBuilder<AgreementDocumentVersion> builder)
    {
        builder.Property(t => t.Title).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Version).HasMaxLength(10).IsRequired();
        builder.Property(t => t.LanguageCode).HasMaxLength(10).IsRequired();
        builder.HasIndex(t => t.AgreementDocumentId);
        builder.Property(t => t.IsOptional).IsRequired();
    }
}