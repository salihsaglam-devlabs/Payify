using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantPoolConfiguration : IEntityTypeConfiguration<MerchantPool>
{
    public void Configure(EntityTypeBuilder<MerchantPool> builder)
    {
        builder.Property(b => b.MerchantPoolStatus).IsRequired();
        builder.Property(b => b.MerchantType).IsRequired();
        builder.Property(b => b.ParentMerchantName).HasMaxLength(150);
        builder.Property(b => b.ParentMerchantNumber).HasMaxLength(15);
        builder.Property(b => b.CompanyType).IsRequired();
        builder.Property(b => b.CommercialTitle).IsRequired().HasMaxLength(100);
        builder.Property(b => b.WebSiteUrl).IsRequired().HasMaxLength(100);
        builder.Property(b => b.MonthlyTurnover).IsRequired().HasPrecision(18,4);
        builder.Property(b => b.PostalCode).IsRequired().HasMaxLength(5);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(256);
        builder.Property(b => b.PhoneCode).IsRequired().HasMaxLength(15);
        builder.Property(b => b.Country).IsRequired();
        builder.Property(b => b.CountryName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.City).IsRequired();
        builder.Property(b => b.CityName).IsRequired().HasMaxLength(200); 
        builder.Property(b => b.District).IsRequired();
        builder.Property(b => b.DistrictName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.TaxAdministration).IsRequired().HasMaxLength(200);
        builder.Property(b => b.RejectReason).HasMaxLength(256);
        builder.Property(b => b.TaxNumber).IsRequired().HasMaxLength(11);
        builder.Property(b => b.Channel).HasMaxLength(50);
        builder.Property(b => b.TradeRegistrationNumber).IsRequired().HasMaxLength(16);
        builder.Property(b => b.Iban).HasMaxLength(26);
        builder.Property(b => b.Email).IsRequired().HasMaxLength(256);
        builder.Property(b => b.CompanyEmail).IsRequired().HasMaxLength(256);
        builder.Property(b => b.AuthorizedPersonIdentityNumber).HasMaxLength(11);
        builder.Property(b => b.AuthorizedPersonName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.AuthorizedPersonSurname).IsRequired().HasMaxLength(100);
        builder.Property(b => b.AuthorizedPersonBirthDate).IsRequired();
        builder.Property(b => b.AuthorizedPersonCompanyPhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.AuthorizedPersonMobilePhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.AuthorizedPersonMobilePhoneNumberSecond).HasMaxLength(20);
        builder.Property(x => x.ParameterValue).HasMaxLength(100);
        builder.Property(b => b.PostingPaymentChannel).IsRequired().HasDefaultValue(PostingPaymentChannel.BankAccount);
        builder.Property(b => b.WalletNumber).HasMaxLength(26);
        builder.Property(b => b.IsPaymentToMainMerchant).IsRequired().HasDefaultValue(false);
        builder.Property(b => b.PosType).IsRequired().HasDefaultValue(PosType.Virtual);
        builder.Property(b => b.MoneyTransferStartHour).IsRequired(false);
        builder.Property(b => b.MoneyTransferStartMinute).IsRequired(false);

        builder
        .HasOne(b => b.Currency)
        .WithMany()
        .HasForeignKey(b => b.CurrencyCode)
        .HasPrincipalKey(b => b.Code)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
