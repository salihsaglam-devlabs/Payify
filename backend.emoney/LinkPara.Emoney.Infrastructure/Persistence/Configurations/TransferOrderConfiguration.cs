using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class TransferOrderConfiguration : IEntityTypeConfiguration<TransferOrder>
{
    public void Configure(EntityTypeBuilder<TransferOrder> builder)
    {
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.SenderWalletNumber).IsRequired().HasMaxLength(10);
        
        builder.Property(t => t.ReceiverAccountType).IsRequired();
        builder.Property(t => t.ReceiverAccountValue).IsRequired().HasMaxLength(50);
        builder.Property(t => t.SenderNameSurname).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ReceiverNameSurname).IsRequired(false).HasMaxLength(50);
        builder.Property(t => t.ReceiverWalletNumber).IsRequired(false).HasMaxLength(50);

        builder.Property(t => t.TransferDate).IsRequired().HasColumnType("date");
        builder.Property(s => s.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Description).IsRequired(false).HasMaxLength(100);
        builder.Property(t => t.PaymentType).HasMaxLength(100);

        builder.Property(t => t.TransferOrderStatus).IsRequired();
        builder.Property(s => s.ReceiverPhoneCode).HasMaxLength(10);
        builder.Property(s => s.CurrencyCode).IsRequired().HasMaxLength(10);
        builder.HasIndex(s => s.TransferDate );
        builder.HasIndex(s => s.SenderWalletNumber);
        builder.HasIndex(s => s.CurrencyCode);
        builder.Property(s => s.ErrorMessage).HasMaxLength(500);

        builder
        .HasOne(s => s.Currency)
        .WithMany()
        .HasForeignKey(s => s.CurrencyCode)
        .HasPrincipalKey(s => s.Code)
        .OnDelete(DeleteBehavior.Restrict);
    }
}