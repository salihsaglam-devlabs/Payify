using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace LinkPara.PF.Infrastructure.Persistence.Configurations
{
    public class MerchantBusinessPartnerConfiguration : IEntityTypeConfiguration<MerchantBusinessPartner>
    {
        public void Configure(EntityTypeBuilder<MerchantBusinessPartner> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.IdentityNumber).HasMaxLength(20).IsRequired();
            builder.Property(u => u.PhoneNumber).HasMaxLength(50).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.Property(u => u.AmlReferenceNumber).HasMaxLength(150);
            builder.Property(u => u.BirthDate).IsRequired();

            builder.Property(b => b.MerchantId).IsRequired();
        }
    }
}