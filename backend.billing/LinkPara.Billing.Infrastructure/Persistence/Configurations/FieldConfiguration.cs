using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.Property(s => s.Label).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Mask).HasMaxLength(100);
        builder.Property(s => s.Pattern).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Placeholder).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Length).IsRequired();
        builder.Property(s => s.Order).IsRequired();
        builder.Property(s => s.InstitutionId).IsRequired();
        builder.Property(s => s.Prefix).HasMaxLength(100);
        builder.Property(s => s.Suffix).HasMaxLength(100);

        builder.HasIndex(s => new { s.Label, s.InstitutionId });
    }
}