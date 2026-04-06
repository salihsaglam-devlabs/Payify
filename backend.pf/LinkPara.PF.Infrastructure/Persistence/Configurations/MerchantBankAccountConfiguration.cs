using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantBankAccountConfiguration : IEntityTypeConfiguration<MerchantBankAccount>
{
    public void Configure(EntityTypeBuilder<MerchantBankAccount> builder)
    {
        builder.Property(b => b.Iban).IsRequired().HasMaxLength(26);

        builder
          .HasOne(s => s.Bank)
          .WithMany()
          .HasForeignKey(s => s.BankCode)
          .HasPrincipalKey(s => s.Code)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
