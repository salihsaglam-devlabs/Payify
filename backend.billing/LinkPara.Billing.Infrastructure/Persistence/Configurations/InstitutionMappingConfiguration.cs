using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class InstitutionMappingConfiguration : IEntityTypeConfiguration<InstitutionMapping>
{
    public void Configure(EntityTypeBuilder<InstitutionMapping> builder)
    {
        builder.Property(b => b.InstitutionId).IsRequired();
        builder.Property(b => b.VendorInstitutionId).HasMaxLength(100).IsRequired();
        builder.Property(b => b.VendorId).IsRequired();

        builder.HasIndex(b => new { b.InstitutionId, b.VendorId });
    }
}