using LinkPara.Epin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Epin.Infrastructure.Persistence.Configurations;

public class ReconciliationDetailConfiguration : IEntityTypeConfiguration<ReconciliationDetail>
{
    public void Configure(EntityTypeBuilder<ReconciliationDetail> builder)
    {

        builder.Property(t => t.ExternalTotal).HasPrecision(18, 4);
        builder.Property(s => s.InternalOrderErrorMessage).HasMaxLength(300);
        builder.Property(s => s.ProductName).HasMaxLength(300);

        builder
         .HasOne(s => s.ReconciliationSummary)
         .WithMany()
         .HasForeignKey(s => s.ReconciliationSummaryId)
         .HasPrincipalKey(s => s.Id)
         .OnDelete(DeleteBehavior.Restrict);

        builder
         .HasOne(s => s.Order)
         .WithMany()
         .HasForeignKey(s => s.OrderId)
         .IsRequired(false)
         .HasPrincipalKey(s => s.Id)
         .OnDelete(DeleteBehavior.ClientNoAction);

        builder
         .HasOne(s => s.OrderHistory)
         .WithMany()
         .HasForeignKey(s => s.OrderHistoryId)
         .IsRequired(false)
         .HasPrincipalKey(s => s.Id)
         .OnDelete(DeleteBehavior.ClientNoAction);

    }
}
