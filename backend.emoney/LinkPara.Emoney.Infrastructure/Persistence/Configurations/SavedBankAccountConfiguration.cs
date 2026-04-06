using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class SavedBankAccountConfiguration : IEntityTypeConfiguration<SavedBankAccount>
{
    public void Configure(EntityTypeBuilder<SavedBankAccount> builder)
    {
        builder.Property(t => t.Iban).HasMaxLength(50);
        builder.HasIndex(t => t.BankId);
    }
}