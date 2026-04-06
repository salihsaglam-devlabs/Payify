using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SectorId).IsRequired();
        builder.Property(s => s.ActiveVendorId).IsRequired();
        builder.Property(s => s.OperationMode).IsRequired();
        builder.Property(s => s.FieldRequirementType).IsRequired();
        builder.HasIndex(s => s.ActiveVendorId);
        builder.HasIndex(s => s.SectorId);

        builder.HasOne(s => s.ActiveVendor)
            .WithMany()
            .HasForeignKey(s => s.ActiveVendorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}