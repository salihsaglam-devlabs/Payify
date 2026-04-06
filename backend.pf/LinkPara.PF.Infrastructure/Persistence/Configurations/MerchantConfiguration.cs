using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Number).IsRequired().HasMaxLength(15);
        builder.Property(b => b.ParentMerchantName).HasMaxLength(150);
        builder.Property(b => b.ParentMerchantNumber).HasMaxLength(15);
        builder.Property(b => b.MerchantStatus).IsRequired();
        builder.Property(b => b.MerchantType).IsRequired();
        builder.Property(b => b.IntegrationMode).IsRequired();
        builder.Property(b => b.PhoneCode).IsRequired().HasMaxLength(15);
        builder.Property(b => b.Language).HasMaxLength(100);
        builder.Property(b => b.WebSiteUrl).IsRequired().HasMaxLength(150);
        builder.Property(b => b.MonthlyTurnover).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.RejectReason).HasMaxLength(256);
        builder.Property(b => b.PricingProfileNumber).HasMaxLength(6);
        builder.Property(x => x.ParameterValue).HasMaxLength(100);
        builder.Property(x => x.AnnulmentCode).HasMaxLength(2);
        builder.Property(x => x.AnnulmentDescription).HasMaxLength(300);
        builder.Property(x => x.GlobalMerchantId).HasMaxLength(8);
        builder.Property(x => x.AnnulmentId).HasMaxLength(6);
        builder.Property(x => x.AnnulmentAdditionalInfo).HasMaxLength(300);
        builder.Property(b => b.HostingTaxNo).HasMaxLength(11);
        builder.Property(b => b.HostingTradeName).HasMaxLength(150);
        builder.Property(b => b.Information).HasMaxLength(300);
        builder.Property(b => b.HostingUrl).HasMaxLength(150);
        builder.Property(b => b.PostingPaymentChannel).IsRequired().HasDefaultValue(PostingPaymentChannel.BankAccount);
        builder.Property(b => b.IsPaymentToMainMerchant).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.InsurancePaymentAllowed).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.EstablishmentDate).IsRequired();
        builder.Property(b => b.BusinessModel).IsRequired();
        builder.Property(b => b.BusinessActivity).IsRequired().HasMaxLength(140);
        builder.Property(b => b.BranchCount).IsRequired();
        builder.Property(b => b.EmployeeCount).IsRequired();
        builder.Property(b => b.PosType).IsRequired().HasDefaultValue(PosType.Virtual);
        builder.Property(b => b.MoneyTransferStartHour).IsRequired(false);
        builder.Property(b => b.MoneyTransferStartMinute).IsRequired(false);

        builder
          .HasOne(s => s.Mcc)
          .WithMany()
          .HasForeignKey(s => s.MccCode)
          .HasPrincipalKey(s => s.Code)
          .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(s => s.Nace)
            .WithMany()
            .HasForeignKey(s => s.NaceCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder
         .HasMany(b => b.MerchantUsers)
         .WithOne(b => b.Merchant)
         .IsRequired();

        builder
         .HasMany(b => b.MerchantDocuments)
         .WithOne(b => b.Merchant)
         .IsRequired();

        builder
           .HasMany(b => b.MerchantVposList)
           .WithOne(b => b.Merchant)
           .IsRequired();

        builder
        .HasMany(b => b.MerchantScores)
        .WithOne(b => b.Merchant)
        .IsRequired();

        builder
         .HasMany(b => b.MerchantBankAccounts)
         .WithOne(b => b.Merchant);

        builder
           .HasMany(b => b.MerchantEmails)
           .WithOne(b => b.Merchant)
           .IsRequired();

        builder
          .HasMany(b => b.MerchantLimits)
          .WithOne(b => b.Merchant)
          .IsRequired();

        builder.Ignore(t => t.DomainEvents);

        builder
        .HasMany(b => b.MerchantBusinessPartner)
        .WithOne(b => b.Merchant)
        .IsRequired();

        builder
            .HasMany(b => b.MerchantWallets)
            .WithOne(b => b.Merchant);
    }
}
