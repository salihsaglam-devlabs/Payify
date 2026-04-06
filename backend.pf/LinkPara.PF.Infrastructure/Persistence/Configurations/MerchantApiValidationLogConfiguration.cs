using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantApiValidationLogConfiguration : IEntityTypeConfiguration<MerchantApiValidationLog>
{
    public void Configure(EntityTypeBuilder<MerchantApiValidationLog> builder)
    {
        builder.Property(b => b.TransactionType).IsRequired();
        builder.Property(b => b.ErrorCode).IsRequired().HasMaxLength(10);
        builder.Property(b => b.ErrorMessage).IsRequired().HasMaxLength(256);
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.PointAmount).HasPrecision(18, 4);
        builder.Property(b => b.CardToken).HasMaxLength(50);
        builder.Property(b => b.Currency).HasMaxLength(50);
        builder.Property(b => b.ThreeDSessionId).HasMaxLength(200);
        builder.Property(b => b.ConversationId).HasMaxLength(50);
        builder.Property(b => b.OriginalReferenceNumber).HasMaxLength(50);
        builder.Property(b => b.ClientIpAddress).HasMaxLength(50);
        builder.Property(b => b.LanguageCode).HasMaxLength(100);
        builder.Property(b => b.ApiName).HasMaxLength(200);
    }
}
