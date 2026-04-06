using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(b => b.CustomerStatus).IsRequired();
        builder.Property(b => b.MersisNumber).HasMaxLength(16);
        builder.Property(b => b.CompanyType).IsRequired();
        builder.Property(b => b.CommercialTitle).IsRequired().HasMaxLength(100);
        builder.Property(b => b.TradeRegistrationNumber).IsRequired().HasMaxLength(16);
        builder.Property(b => b.Country).IsRequired();
        builder.Property(b => b.CountryName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.City).IsRequired();
        builder.Property(b => b.CityName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.District).IsRequired();
        builder.Property(b => b.DistrictName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.PostalCode).IsRequired().HasMaxLength(5);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(256);
        builder.Property(b => b.TaxAdministration).IsRequired().HasMaxLength(200);
        builder.Property(b => b.TaxNumber).IsRequired().HasMaxLength(11);
        builder.Property(b => b.CustomerId).IsRequired();
        builder.Property(b => b.CustomerNumber).IsRequired();
        builder.Property(b => b.ExternalCustomerId).HasDefaultValue(Guid.Empty);

        builder
        .HasMany(b => b.Merchants)
        .WithOne(b => b.Customer)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
