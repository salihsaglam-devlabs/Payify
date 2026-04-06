using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ChangedBalanceLogConfiguration : IEntityTypeConfiguration<ChangedBalanceLog>
{
    public void Configure(EntityTypeBuilder<ChangedBalanceLog> builder)
    {
        builder
            .HasOne(s => s.Wallet)
            .WithMany()
            .HasForeignKey(s => s.WalletId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
