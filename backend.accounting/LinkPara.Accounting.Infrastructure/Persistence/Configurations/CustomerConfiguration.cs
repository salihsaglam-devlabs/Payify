using LinkPara.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Accounting.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.Property(s => s.FirstName).HasMaxLength(100);
        builder.Property(s => s.LastName).HasMaxLength(100);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.PhoneNumber).HasMaxLength(20);
        builder.Property(s => s.PhoneCode).HasMaxLength(20);
        builder.Property(s => s.IdentityNumber).HasMaxLength(20);
        builder.Property(s => s.Title).HasMaxLength(100);
        builder.Property(s => s.CurrencyCode).HasMaxLength(20);
        builder.Property(s => s.ResultMessage).HasMaxLength(800);
        builder.Property(s => s.City).HasMaxLength(50);
        builder.Property(s => s.CityCode).HasMaxLength(15);
        builder.Property(s => s.Country).HasMaxLength(50);
        builder.Property(s => s.CountryCode).HasMaxLength(15);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.TaxNumber).HasMaxLength(11);
        builder.Property(s => s.TaxOffice).HasMaxLength(50);
        builder.Property(s => s.TaxOfficeCode).HasMaxLength(20);
        builder.Property(s => s.Town).HasMaxLength(50);
        builder.Property(s => s.District).HasMaxLength(50);
        builder.Property(s => s.CustomerCode).HasMaxLength(20);
        builder.Property(s => s.AccountingCustomerCode).HasMaxLength(20);

        builder.HasIndex(s => s.Code);
    }
}
