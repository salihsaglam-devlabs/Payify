using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class ContactPersonConfiguration : IEntityTypeConfiguration<ContactPerson>
{
    public void Configure(EntityTypeBuilder<ContactPerson> builder)
    {
        builder.Property(b => b.Email).IsRequired().HasMaxLength(256);
        builder.Property(b => b.CompanyEmail).HasMaxLength(256);
        builder.Property(b => b.ContactPersonType).IsRequired();
        builder.Property(b => b.IdentityNumber).HasMaxLength(11);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Surname).IsRequired().HasMaxLength(100);
        builder.Property(b => b.BirthDate).IsRequired();
        builder.Property(b => b.CompanyPhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.MobilePhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.MobilePhoneNumberSecond).HasMaxLength(20);
        builder.Property(b => b.ExternalPersonId).HasDefaultValue(Guid.Empty);
    }
}
