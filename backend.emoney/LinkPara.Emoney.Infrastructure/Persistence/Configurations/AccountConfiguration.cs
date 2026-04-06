using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(s => s.CustomerId);
        builder.Property(s => s.AccountStatus).IsRequired();
        builder.Property(s => s.AccountType).IsRequired();
        builder.Property(s => s.AccountKycLevel).IsRequired();
        builder.Property(s => s.OpeningDate).IsRequired();
        builder.Property(s => s.ChangeReason).HasMaxLength(300);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.PhoneCode).HasMaxLength(10);
        builder.Property(s => s.PhoneNumber).HasMaxLength(50);
        builder.Property(s => s.IdentityNumber).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.IsCommercial).HasDefaultValue(false);
        builder.Property(s => s.BirthDate).HasDefaultValue(default(DateTime));
        builder.HasIndex(s => s.IdentityNumber).IsUnique();
        builder.Property(s => s.VirtualIban).HasMaxLength(30);
        builder.Property(s => s.IsAddressConfirmed).HasDefaultValue(false);
        builder.Property(s => s.ParentAccountId).HasDefaultValue(null);
        builder.Property(s => s.IsOpenBankingPermit).HasDefaultValue(false);
        builder.Property(s => s.Profession).HasMaxLength(150);
        builder.Property(s => s.DeclarationStatus).HasMaxLength(20);
    }
}
