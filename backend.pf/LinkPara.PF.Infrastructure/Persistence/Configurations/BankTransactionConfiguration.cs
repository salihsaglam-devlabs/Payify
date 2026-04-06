using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.Property(b => b.TransactionStatus).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TransactionType).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TransactionStartDate).IsRequired();
        builder.Property(b => b.TransactionEndDate);
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.PointAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.InstallmentCount).IsRequired();
        builder.Property(b => b.CardNumber).HasMaxLength(50);
        builder.Property(b => b.IsReverse).IsRequired();
        builder.Property(b => b.ReverseDate);
        builder.Property(b => b.Is3ds).IsRequired();
        builder.Property(b => b.IssuerBankCode);
        builder.Property(b => b.AcquireBankCode);
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.BankOrderId).HasMaxLength(50);
        builder.Property(b => b.RrnNumber).HasMaxLength(50);
        builder.Property(b => b.Stan).HasMaxLength(50);
        builder.Property(b => b.ApprovalCode).HasMaxLength(50);
        builder.Property(b => b.BankResponseCode).HasMaxLength(50);
        builder.Property(b => b.BankResponseDescription).HasMaxLength(1000);
        builder.Property(b => b.MerchantCode).HasMaxLength(200);
        builder.Property(b => b.SubMerchantCode).HasMaxLength(200);        
        builder.Property(b => b.EndOfDayStatus).IsRequired().HasDefaultValue(EndOfDayStatus.Pending);

        builder
         .HasOne(s => s.AcquireBank)
         .WithMany()
         .HasForeignKey(s => s.AcquireBankCode)
         .HasPrincipalKey(s => s.Code)
         .OnDelete(DeleteBehavior.Restrict);

        builder
         .HasOne(s => s.IssuerBank)
         .WithMany()
         .HasForeignKey(s => s.IssuerBankCode)
         .HasPrincipalKey(s => s.Code)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
