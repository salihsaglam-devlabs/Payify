using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class CompanyPoolConfiguration : IEntityTypeConfiguration<CompanyPool>
{
    public void Configure(EntityTypeBuilder<CompanyPool> builder)
    {
        builder.Property(b => b.CompanyPoolStatus).IsRequired();
        builder.Property(b => b.CompanyType).IsRequired();
        builder.Property(b => b.Title).IsRequired().HasMaxLength(256);
        builder.Property(b => b.WebSiteUrl).HasMaxLength(100);
        builder.Property(b => b.PostalCode).IsRequired().HasMaxLength(5);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(256);
        builder.Property(b => b.PhoneCode).IsRequired().HasMaxLength(15);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(b => b.LandPhone).HasMaxLength(15);
        builder.Property(b => b.Country).IsRequired();
        builder.Property(b => b.CountryName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.City).IsRequired();
        builder.Property(b => b.CityName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.District).IsRequired();
        builder.Property(b => b.DistrictName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.TaxAdministration).IsRequired().HasMaxLength(200);
        builder.Property(b => b.RejectReason).HasMaxLength(256);
        builder.Property(b => b.TaxNumber).IsRequired().HasMaxLength(11);
        builder.Property(b => b.Iban).HasMaxLength(26);
        builder.Property(b => b.Email).IsRequired().HasMaxLength(256);
        builder.Property(b => b.AuthorizedPersonIdentityNumber).HasMaxLength(11);
        builder.Property(b => b.AuthorizedPersonName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.AuthorizedPersonSurname).IsRequired().HasMaxLength(100);
        builder.Property(b => b.AuthorizedPersonBirthDate).IsRequired();
        builder.Property(b => b.AuthorizedPersonCompanyPhoneNumber).IsRequired().HasMaxLength(15);
        builder.Property(b => b.AuthorizedPersonCompanyPhoneCode).IsRequired().HasMaxLength(15);
        builder.Property(b => b.AuthorizedPersonEmail).IsRequired().HasMaxLength(256);
        builder.Property(b => b.MersisNumber).HasMaxLength(16);

    }
}
