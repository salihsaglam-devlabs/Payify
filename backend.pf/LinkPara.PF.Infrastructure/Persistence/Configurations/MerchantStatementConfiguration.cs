using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantStatementConfiguration : IEntityTypeConfiguration<MerchantStatement>
{
    public void Configure(EntityTypeBuilder<MerchantStatement> builder)
    {
        builder.Property(b => b.Id).IsRequired();
        builder.Property(b => b.MerchantId).IsRequired();
        builder.Property(b => b.StatementStartDate).IsRequired();
        builder.Property(b => b.StatementEndDate).IsRequired();
        builder.Property(b => b.MailAddress).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ExcelPath).HasMaxLength(256);
        builder.Property(b => b.PdfPath).HasMaxLength(256);
        builder.Property(b => b.FileName).HasMaxLength(50);
        builder.Property(b => b.ReceiptNumber).HasMaxLength(50);
        builder.Property(b => b.StatementStatus).IsRequired();
        builder.Property(b => b.StatementType).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(300);
        builder.Property(b => b.StatementMonth);
        builder.Property(b => b.StatementYear);
        builder.Property(b => b.MerchantName);
    }
}
