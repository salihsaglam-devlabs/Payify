using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ProvisionConfiguration : IEntityTypeConfiguration<Provision>
{
    public void Configure(EntityTypeBuilder<Provision> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.CurrencyCode).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Description).HasMaxLength(300);
        builder.Property(t => t.ConversationId).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ProvisionSource).IsRequired();
        builder.Property(t => t.ProvisionStatus).IsRequired();
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.WalletNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ClientIpAddress).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ProvisionReference).IsRequired().HasMaxLength(15).HasDefaultValue("000000000000000");
        builder.Property(t => t.CommissionAmount).HasPrecision(18, 2);
        builder.Property(t => t.BsmvAmount).HasPrecision(18, 2);

        builder.HasIndex(t=> t.ConversationId);
        builder.HasIndex(t=> t.TransactionId);
        builder.HasIndex(t => t.ProvisionReference);

        builder.HasOne(s => s.Partner)
               .WithMany()
               .HasForeignKey(s => s.PartnerId)
               .IsRequired(false)
               .HasPrincipalKey(s => s.Id)
               .OnDelete(DeleteBehavior.ClientNoAction);
        
        builder.HasOne(s => s.PaymentProvision)
               .WithMany()
               .HasForeignKey(s => s.PaymentProvisionId)
               .IsRequired(false)
               .HasPrincipalKey(s => s.Id)
               .OnDelete(DeleteBehavior.ClientNoAction);
    }
}