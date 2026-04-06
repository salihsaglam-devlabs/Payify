using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountIbanConfiguration : IEntityTypeConfiguration<AccountIban>
{
    public void Configure(EntityTypeBuilder<AccountIban> builder)
    {
        builder.Property(s => s.Iban).IsRequired().HasMaxLength(50);
        builder.Property(s => s.IdentityNo).IsRequired().HasMaxLength(20);
        
        builder.HasIndex(t => new { t.Iban, t.IdentityNo });
    }
}