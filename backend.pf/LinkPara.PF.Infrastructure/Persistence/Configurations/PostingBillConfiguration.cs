using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class PostingBillConfiguration : IEntityTypeConfiguration<PostingBill>
{
    public void Configure(EntityTypeBuilder<PostingBill> builder)
    {
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPayingAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalDueAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalPfCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalBankCommissionAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.ClientReferenceId).IsRequired();
        builder.Property(b => b.BillDate).IsRequired().HasColumnType("date");
        builder.Property(b => b.BillMonth).IsRequired();
        builder.Property(b => b.BillYear).IsRequired();
        builder.Property(b => b.Currency).IsRequired();

        builder.HasIndex(b => new { b.MerchantId, b.BillMonth, b.BillYear });
    }
}
