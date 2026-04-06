using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class InstitutionDetailConfiguration : IEntityTypeConfiguration<InstitutionDetail>
{
    public void Configure(EntityTypeBuilder<InstitutionDetail> builder)
    {
        builder.Property(s => s.InstitutionSummaryId).IsRequired();
        builder.Property(s => s.VendorId).IsRequired();
        builder.Property(s => s.InstitutionId).IsRequired();
        builder.Property(s => s.ReconciliationDate).IsRequired().HasColumnType("date"); ;
        builder.Property(s => s.ReconciliationStatus).IsRequired();
        builder.Property(s => s.PaymentStatus).IsRequired();
        builder.Property(s => s.BillDate).HasColumnType("date");
        builder.Property(s => s.BillDueDate).HasColumnType("date");
    }
}