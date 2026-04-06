using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class SavedWalletAccountConfiguration : IEntityTypeConfiguration<SavedWalletAccount>
{
    public void Configure(EntityTypeBuilder<SavedWalletAccount> builder)
    {
        builder.Property(t => t.WalletNumber).HasMaxLength(10);
        builder.HasIndex(t => t.UserId);
    }
}
