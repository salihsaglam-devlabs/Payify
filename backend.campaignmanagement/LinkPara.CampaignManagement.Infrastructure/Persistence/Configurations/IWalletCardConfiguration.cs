using LinkPara.CampaignManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Configurations;

public class IWalletCardConfiguration : IEntityTypeConfiguration<IWalletCard>
{
    public void Configure(EntityTypeBuilder<IWalletCard> builder)
    {
        builder.Property(s => s.WalletNumber).HasMaxLength(100).IsRequired();
        builder.Property(s => s.FullName).HasMaxLength(500).IsRequired();
        builder.Property(s => s.IdentityNumber).HasMaxLength(500).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(500).IsRequired();
        builder.Property(s => s.PhoneNumber).HasMaxLength(500).IsRequired();
        builder.Property(s => s.AddressDetail).HasMaxLength(1500).IsRequired();
        builder.Property(s => s.IndividualFrameworkAgreementVersion).HasMaxLength(100).IsRequired();
        builder.Property(s => s.PreliminaryInformationAgreementVersion).HasMaxLength(100).IsRequired();
        builder.Property(s => s.KvkkAgreementVersion).HasMaxLength(100).IsRequired();
        builder.Property(s => s.CommercialElectronicCommunicationAggrementVersion).HasMaxLength(100).IsRequired();
        builder.Property(s => s.CardNumber).HasMaxLength(500);
        builder.Property(s => s.ErrorMessage).HasMaxLength(800);

        builder.HasIndex(s => new { s.UserId, s.WalletNumber });
    }
}