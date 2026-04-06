using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations
{
    public class WalletBalanceDailyConfiguration : IEntityTypeConfiguration<WalletBalanceDaily>
    {
        public void Configure(EntityTypeBuilder<WalletBalanceDaily> builder)
        {
            builder.Property(s => s.JobDate)
                .IsRequired();

            builder.Property(s => s.DailyBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.Currency)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(s => s.Difference)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        }
    }
}