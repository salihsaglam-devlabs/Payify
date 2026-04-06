using LinkPara.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Billing.Infrastructure.Persistence.Configurations;

public class SavedBillConfiguration : IEntityTypeConfiguration<SavedBill>
{
    public void Configure(EntityTypeBuilder<SavedBill> builder)
    {
        builder.Property(s => s.InstitutionId).IsRequired();
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.SubscriberNumber1).HasMaxLength(50).IsRequired();
        builder.Property(s => s.SubscriberNumber2).HasMaxLength(50);
        builder.Property(s => s.SubscriberNumber3).HasMaxLength(50);
        builder.Property(s => s.BillName).HasMaxLength(50).IsRequired();
    }
}