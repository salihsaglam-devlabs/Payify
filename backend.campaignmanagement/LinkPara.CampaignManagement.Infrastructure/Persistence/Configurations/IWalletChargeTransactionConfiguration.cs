using LinkPara.CampaignManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Configurations;

public class IWalletChargeTransactionConfiguration : IEntityTypeConfiguration<IWalletChargeTransaction>
{
    public void Configure(EntityTypeBuilder<IWalletChargeTransaction> builder)
    {
        builder.Property(s => s.Amount).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.ChargeTransactionType).IsRequired();
        builder.Property(s => s.ProvisionConversationId).HasMaxLength(50);
        builder.Property(s => s.ProvisionReferenceNumber).HasMaxLength(50);

        builder
            .HasOne(s => s.IWalletCharge)
            .WithMany()
            .HasForeignKey(s => s.IWalletChargeId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
