using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations
{
    public class AccountFinancialInfoConfiguration : IEntityTypeConfiguration<AccountFinancialInformation>
    {
        public void Configure(EntityTypeBuilder<AccountFinancialInformation> builder)
        {
            builder.Property(s => s.AccountId).IsRequired();
            builder.Property(s => s.IncomeSource).HasMaxLength(50).IsRequired();
            builder.Property(s => s.IncomeInformation).HasMaxLength(50).IsRequired();
            builder.Property(s => s.MonthlyTransactionCount).HasMaxLength(20).IsRequired();
            builder.Property(s => s.MonthlyTransactionVolume).HasMaxLength(50).IsRequired();

            builder
            .HasOne(s => s.Account)
            .WithOne(s => s.AccountFinancialInformation)
            .HasForeignKey<AccountFinancialInformation>(s => s.AccountId);
        }
    }
}
