using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class TimeoutTransactionConfiguration : IEntityTypeConfiguration<TimeoutTransaction>
{
    public void Configure(EntityTypeBuilder<TimeoutTransaction> builder)
    {
        builder.Property(b => b.TimeoutTransactionStatus).IsRequired();
        builder.Property(b => b.TransactionType).IsRequired();
        builder.Property(b => b.OriginalOrderId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ConversationId).HasMaxLength(50);
        builder.Property(b => b.OrderId).HasMaxLength(50);
        builder.Property(b => b.ErrorCode).HasMaxLength(10);
        builder.Property(b => b.ErrorMessage).HasMaxLength(256);
        builder.Property(b => b.ResponseCode).HasMaxLength(10);
        builder.Property(b => b.ResponseMessage).HasMaxLength(256);
        builder.Property(b => b.PosErrorCode).HasMaxLength(20);
        builder.Property(b => b.PosErrorMessage).HasMaxLength(1000);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(256);
        builder.Property(b => b.LanguageCode).HasMaxLength(100);
        builder.Property(b => b.ClientIpAddress).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.CardNumber).HasMaxLength(50);
        builder.Property(b => b.SubMerchantCode).HasMaxLength(200);

        builder
        .HasOne(s => s.AcquireBank)
        .WithMany()
        .HasForeignKey(s => s.AcquireBankCode)
        .HasPrincipalKey(s => s.Code)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
